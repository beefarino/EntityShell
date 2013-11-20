using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Metadata.Edm;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using CodeOwls.EntityProvider.Attributes;
using PropertyAttributes = System.Reflection.PropertyAttributes;

namespace CodeOwls.EntityProvider.Adapters
{
    public interface IEntityAdapter<T>
    {
        T ToEntity();
        T ToNewEntity(DbContext context);
        void CopyTo(T entity);
    }

    [Serializable]
    public class EntityStateValidationException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public EntityStateValidationException()
        {
        }

        public EntityStateValidationException(string message) : base(message)
        {
        }

        public EntityStateValidationException(string message, Exception inner) : base(message, inner)
        {
        }

        protected EntityStateValidationException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
    class EntityRuntimeParameterAdapter
    {
        private static AssemblyBuilder _assemblyBuilder;
        private static ModuleBuilder _moduleBuilder;
        private static AssemblyName _dynamicAssemblyName;
        private static string _rootNamespace;
        private static readonly Version DynamicAssemblyVersion = new Version(1, 1, 0, 0);
        private bool _specifyMandatoryParameters;

        public EntityRuntimeParameterAdapter()
        {
            _specifyMandatoryParameters = true;
        }

        public object AsNewItemRuntimeParameter<T>(T entity, EntityType entityMetadata)
        {
            if (null == entity)
            {
                return null;
            }

            _specifyMandatoryParameters = true;
            return AsRuntimeParameter(entity, entityMetadata, "New");
        }

        public object AsSetItemRuntimeParameter<T>(T entity, EntityType entityMetadata)
        {
            if (null == entity)
            {
                return null;
            }

            _specifyMandatoryParameters = false;

            return AsRuntimeParameter(entity, entityMetadata, "Set");
        }

        private object AsRuntimeParameter<T>(T entity, EntityType entityMetadata, string classNamePrefix)
        {
            InitializeDynamicAssembly(entityMetadata);

            var type = entity.GetType();
            string typename = _rootNamespace + "." + classNamePrefix + "." + type.Name;

            Type parameterType = null;
            lock (_assemblyBuilder)
            {
                parameterType = _assemblyBuilder.GetType(typename, false);
                if (null == parameterType)
                {
                    try
                    {
                        TypeBuilder typeBuilder = _moduleBuilder.DefineType(typename, TypeAttributes.Public, null,
                                                                           new[] {typeof (IEntityAdapter<T>)});
                        typeBuilder.AddInterfaceImplementation(typeof (IEntityAdapter<T>));
                        var fieldBuilder = typeBuilder.DefineField("_entity", type,
                                                                   FieldAttributes.Public | FieldAttributes.InitOnly);

                        DefineCtor(typeBuilder, fieldBuilder, type);
                        DefineAccessors(typeBuilder, fieldBuilder, entity, entityMetadata);
                        DefineAdapterMethod<T>(typeBuilder, fieldBuilder);
                        DefineNewItemAdapterMethod<T>(typeBuilder, fieldBuilder);
                        DefineCopyItemAdapterMethod<T>(typeBuilder, fieldBuilder, entityMetadata);
                        parameterType = typeBuilder.CreateType();
                    }
                    catch
                    {
                        parameterType = _assemblyBuilder.GetType(typename, false);
                    }
                }
            }

            if (null == parameterType)
            {
                parameterType = Type.GetType(typename);
            }

            if (null == parameterType)
            {
                return null;
            }

            return Activator.CreateInstance(parameterType, Type.EmptyTypes);
        }


        private static void InitializeDynamicAssembly(EntityType entityMetadata)
        {
            if (null == _dynamicAssemblyName)
            {
                _rootNamespace = entityMetadata.NamespaceName + ".RuntimeParameters";
                _dynamicAssemblyName = new AssemblyName
                {
                    Name = _rootNamespace,
                    Version = DynamicAssemblyVersion,
                    CultureInfo = CultureInfo.CurrentCulture
                };

            }

            if (null == _assemblyBuilder)
            {
                var appDomain = AppDomain.CurrentDomain;
                _assemblyBuilder = appDomain.DefineDynamicAssembly(_dynamicAssemblyName, AssemblyBuilderAccess.RunAndSave);
                _moduleBuilder = _assemblyBuilder.DefineDynamicModule(_dynamicAssemblyName.Name, _dynamicAssemblyName.Name + ".dll");
            }
        }

        private void DefineCtor(TypeBuilder typeBuilder, FieldBuilder fieldBuilder, Type baseType)
        {
            var ctor = baseType.GetConstructor(Type.EmptyTypes);
            var ctorBuilder = typeBuilder.DefineConstructor(
                MethodAttributes.Public, 
                CallingConventions.Standard,
                Type.EmptyTypes
            );

            var generator = ctorBuilder.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Newobj, ctor);            
            generator.Emit(OpCodes.Stfld, fieldBuilder);

            generator.Emit(OpCodes.Ret);            
        }

        void DefineAdapterMethod<T>(TypeBuilder typeBuilder, FieldBuilder fieldBuilder)
        {
            var method = typeBuilder.DefineMethod("ToEntity", MethodAttributes.Public | MethodAttributes.Virtual, typeof(T), Type.EmptyTypes);
            var generator = method.GetILGenerator();

            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldfld, fieldBuilder);
            generator.Emit(OpCodes.Ret);
        }

        void DefineNewItemAdapterMethod<T>(TypeBuilder typeBuilder, FieldBuilder fieldBuilder)
        {

            var method = typeBuilder.DefineMethod("ToNewEntity", MethodAttributes.Public | MethodAttributes.Virtual, typeof(T), new[]{typeof(DbContext)});
            var generator = method.GetILGenerator();

            var ctor = typeof(T).GetConstructor(Type.EmptyTypes);
            
            var entityCopy = generator.DeclareLocal(typeof(T)); 
            
            generator.Emit(OpCodes.Newobj, ctor);
            generator.Emit(OpCodes.Stloc, entityCopy.LocalIndex);
            
            var properties = from p in typeof(T).GetProperties()
                             where p.CanRead && p.CanWrite
                             select p;

            foreach (var propertyInfo in properties)
            {
                DefineVerifyEntityStateIL(generator, entityCopy, propertyInfo, fieldBuilder);
            }
            foreach (var propertyInfo in properties)
            {
                DefineCopyPropertyIL(generator, entityCopy, propertyInfo, fieldBuilder);
            }
            generator.Emit(OpCodes.Ldloc, entityCopy.LocalIndex);
            generator.Emit(OpCodes.Ret);
        }

        private void DefineVerifyEntityStateIL(ILGenerator generator, LocalBuilder entityCopy, PropertyInfo propertyInfo, FieldBuilder fieldBuilder)
        {       
            var contextType = typeof (DbContext);
            var entityEntryType = typeof (DbEntityEntry);
            var entityStateType = typeof (EntityState);
            var label = generator.DefineLabel();
            
            var methodInfo = contextType.GetMethod("Entry", BindingFlags.Instance | BindingFlags.Public, null, new[]{typeof(object)}, null);
            var localState = generator.DeclareLocal(entityStateType);
            var localValue = generator.DeclareLocal(propertyInfo.PropertyType);

            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldfld, fieldBuilder);
            generator.Emit(OpCodes.Call, propertyInfo.GetGetMethod());
            generator.Emit(OpCodes.Stloc, localValue);

            generator.Emit(OpCodes.Ldnull);
            generator.Emit(OpCodes.Ldloc, localValue);    
            generator.Emit(OpCodes.Ceq);
            generator.Emit(OpCodes.Brtrue, label);

            generator.BeginExceptionBlock();
                generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(OpCodes.Ldloc, localValue);    
                generator.Emit(OpCodes.Call, methodInfo);
            
                var stateProperty = entityEntryType.GetProperty("State");
                
                generator.Emit(OpCodes.Call, stateProperty.GetGetMethod());
                generator.Emit(OpCodes.Stloc, localState);
            
                generator.Emit(OpCodes.Ldloc, localState);
            
                generator.Emit(OpCodes.Ldc_I4, (int)EntityState.Detached);

                generator.Emit(OpCodes.Ceq);
                generator.Emit( OpCodes.Brfalse, label );
            
                var ctor = typeof (EntityStateValidationException).GetConstructor(Type.EmptyTypes);
                generator.Emit(OpCodes.Newobj, ctor);
                generator.Emit(OpCodes.Throw);
            
            generator.BeginCatchBlock( typeof( InvalidOperationException ));            
            generator.EndExceptionBlock();
            
            generator.MarkLabel(label);            
        }

        void DefineCopyItemAdapterMethod<T>(TypeBuilder typeBuilder, FieldBuilder fieldBuilder, EntityType entityMetadata)
        {
            var method = typeBuilder.DefineMethod("CopyTo", MethodAttributes.Public | MethodAttributes.Virtual, null, new[]{typeof(T)});
            var generator = method.GetILGenerator();
            var entityCopy = generator.DeclareLocal(typeof (T));
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Stloc, entityCopy.LocalIndex);
            var properties = from p in typeof(T).GetProperties()
                             where p.CanRead && p.CanWrite &&
                                ! (entityMetadata.KeyMembers.Select(k => k.Name).ToArray().Contains(p.Name))
                             select p;

            foreach (var propertyInfo in properties)
            {
                DefineCopyPropertyIL(generator, entityCopy, propertyInfo, fieldBuilder);
            }
            generator.Emit(OpCodes.Ret);
        }


        private void DefineCopyPropertyIL(ILGenerator generator, LocalBuilder entityCopy, PropertyInfo propertyInfo, FieldBuilder fieldBuilder)
        {
            generator.Emit(OpCodes.Ldloc, entityCopy.LocalIndex);
            {
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldfld, fieldBuilder);
                generator.Emit(OpCodes.Call, propertyInfo.GetGetMethod());
            }
            generator.Emit(OpCodes.Call, propertyInfo.GetSetMethod());
        }

        private void DefineAccessors(TypeBuilder typeBuilder, FieldBuilder fieldBuilder, object entity, EntityType entityMetadata)
        {
            var names = entity.GetType().GetProperties();
            foreach (var name in names)
            {
                DefineAccessor(typeBuilder, name, fieldBuilder, entity, entityMetadata);
            }
        }

        private void DefineAccessor(TypeBuilder typeBuilder, PropertyInfo propertyInfo, FieldBuilder fieldBuilder, object entity, EntityType entityMetadata)
        {
            if( null == propertyInfo || propertyInfo.IsSpecialName )
            {
                return;
            }
            var propType = propertyInfo.PropertyType;
            var entityType = entity.GetType();

            var alias = GetSafePropertyName(propertyInfo.Name);
            var name = entityType.Name + propertyInfo.Name;
            DefineAccessor(typeBuilder, propertyInfo, fieldBuilder, name, alias, propType, entityType, entityMetadata);
        }

        private void DefineAccessor(TypeBuilder typeBuilder, PropertyInfo propertyInfo, FieldBuilder fieldBuilder, string name, string alias, Type propType, Type entityType, EntityType entityMetadata)
        {
            var propertyBuilder = typeBuilder.DefineProperty(name, PropertyAttributes.HasDefault, propType, null);
            
            var attributeBuilder = BuildParameterAttribute(propertyInfo, entityMetadata);
            propertyBuilder.SetCustomAttribute(attributeBuilder);

            attributeBuilder = BuildValidationAttribute(propertyInfo);
            propertyBuilder.SetCustomAttribute(attributeBuilder);

            attributeBuilder = BuildAliasAttribute(alias);
            propertyBuilder.SetCustomAttribute( attributeBuilder );

            const MethodAttributes methodAttributes = MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Public;

            if (propertyInfo.CanRead)
            {
                var getProperty = entityType.GetProperty(propertyInfo.Name).GetGetMethod();

                var getter = typeBuilder.DefineMethod("get_" + name, methodAttributes, propType, Type.EmptyTypes);
                var generator = getter.GetILGenerator();
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldfld, fieldBuilder);
                generator.Emit(OpCodes.Call, getProperty);
                generator.Emit(OpCodes.Ret);

                propertyBuilder.SetGetMethod(getter);
            }
            if (propertyInfo.CanWrite)
            {
                var setProperty = entityType.GetProperty(propertyInfo.Name).GetSetMethod();

                var setter = typeBuilder.DefineMethod("set_" + name, methodAttributes, null, new[] {propType});
                var generator = setter.GetILGenerator();
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldfld, fieldBuilder);
                generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(OpCodes.Call, setProperty);

                generator.Emit(OpCodes.Ret);

                propertyBuilder.SetSetMethod(setter);
            }
        }

        private static CustomAttributeBuilder BuildAliasAttribute(string alias)
        {
            return new CustomAttributeBuilder(
                typeof(AliasAttribute).GetConstructor( new []{typeof(string[])}), 
                new[]{new[]{alias}});
        }

        private CustomAttributeBuilder BuildValidationAttribute(PropertyInfo propertyInfo)
        {
            var paramType = typeof (ValidateEntityStateAttribute);
            var builder = new CustomAttributeBuilder(
                paramType.GetConstructor(Type.EmptyTypes),
                new object[] {}
                );

            return builder;
        }

        private CustomAttributeBuilder BuildParameterAttribute(PropertyInfo propertyInfo, EntityType entityMetadata )
        {
            var paramType = typeof (ParameterAttribute);

            var props = new Dictionary<PropertyInfo, object>();
            var vppn = paramType.GetProperty("ValueFromPipelineByPropertyName");
            props.Add(vppn, true);

            // not-nullable members are mandatory
            if (_specifyMandatoryParameters && null !=
                (from p in entityMetadata.Members
                 where (
                           p.Name == propertyInfo.Name &&
                           ! (entityMetadata.KeyMembers.Select(k => k.Name).ToArray().Contains(propertyInfo.Name))
                       )
                 from m in p.TypeUsage.Facets
                 where m.Name == "Nullable" && false.Equals(m.Value)
                 select m)
                    .FirstOrDefault())
            {
                var mpi = paramType.GetProperty("Mandatory");
                props.Add(mpi, true);
            }

            var attributeBuilder = new CustomAttributeBuilder(
                paramType.GetConstructor(Type.EmptyTypes),
                new object[] {},
                props.Keys.ToArray(),
                props.Values.ToArray()
                );
            return attributeBuilder;
        }

        private string GetSafePropertyName(string name)
        {
            // these are names of parameters used by item cmdlets.
            // gcm *item*| select -exp parameters |select -exp keys | sort -unique 
            // in order to prevent powershell from choking, we will
            // need to rename dynamic parameters from the entity that
            // share any of these names
            var fixups = new[]
                             {
                                 "confirm",
                                 "container",
                                 "credential",
                                 "debug",
                                 "destination",
                                 "erroraction",
                                 "errorvariable",
                                 "exclude",
                                 "filter",
                                 "force",
                                 "include",
                                 "inputobject",
                                 "itemtype",
                                 "literalpath",
                                 "name",
                                 "newname",
                                 "outbuffer",
                                 "outvariable",
                                 "passthru",
                                 "path",
                                 "propertytype",
                                 "recurse",
                                 "usetransaction",
                                 "value",
                                 "verbose",
                                 "warningaction",
                                 "warningvariable",
                                 "whatif"

                             };
            if (fixups.Contains(name.ToLowerInvariant()))
            {
                return "_" + name;
            }

            return name;
        }

        public static string SaveAssembly()
        {
            if (null == _assemblyBuilder)
            {
                return null;
            }
            var filePath = _assemblyBuilder.GetName().Name + ".dll";
            _assemblyBuilder.Save( filePath );
            return filePath;
        }

    }
}
