using System;
using System.Linq;
using System.Management.Automation;
using System.Text.RegularExpressions;

namespace CodeOwls.EntityProvider.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class StringToTypeNameArgumentTransformAttribute : ArgumentTransformationAttribute
    {
        public override object Transform(EngineIntrinsics engineIntrinsics, object inputData)
        {
            if (null == inputData )
            {
                return null;
            }

            var typeName = inputData.ToString();
            var re = new Regex(@"^\[[\w\.]+\]$");

            if (!re.IsMatch( typeName ))
            {
                return inputData;
            }

            var results = engineIntrinsics.InvokeCommand.InvokeScript(typeName);
            if (! results.Any())
            {
                return null;
            }

            var type = results.First().BaseObject as Type;
            if (null == type)
            {
                throw new ArgumentException( String.Format(
                    "The string specified '{0}' could not be interpreted as a valid type name.  Please ensure you have loaded the assembly containing the type into your PowerShell session prior to calling this command.",
                    inputData
                                                 )
                    );
            }

            return type;
        }

    }
}