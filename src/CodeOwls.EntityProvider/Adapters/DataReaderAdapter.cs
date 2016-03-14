using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace CodeOwls.EntityProvider.Adapters
{
    class DataReaderAdapter : DbDataReader
    {
        private readonly List<EdmMember> _members;
        private readonly RuntimeDefinedParameterDictionary _item;

        public DataReaderAdapter( IEnumerable<EdmMember> members, RuntimeDefinedParameterDictionary item )
        {
            _members = members.ToList();
            _item = item;
        }

        public override void Close()
        {
        }

        public override DataTable GetSchemaTable()
        {
            return new DataTable();
        }

        public override bool NextResult()
        {
            return true;
        }

        public override bool Read()
        {
            return true;
        }

        public override int Depth
        {
            get { return 1; }
        }

        public override bool IsClosed
        {
            get { return false; }
        }

        public override int RecordsAffected
        {
            get { return 1; }
        }

        public override bool GetBoolean(int ordinal)
        {
            return Convert.ToBoolean(SafeGetProperty(ordinal).Value);
        }

        public override byte GetByte(int ordinal)
        {
            return Convert.ToByte(SafeGetProperty(ordinal).Value);
        }

        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            throw new NotImplementedException();
        }

        public override char GetChar(int ordinal)
        {
            return Convert.ToChar(SafeGetProperty(ordinal).Value);
        }

        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            throw new NotImplementedException();
        }

        public override Guid GetGuid(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override short GetInt16(int ordinal)
        {
            return Convert.ToInt16(SafeGetProperty(ordinal).Value);
        }

        public override int GetInt32(int ordinal)
        {
            return Convert.ToInt32(SafeGetProperty(ordinal).Value);
        }

        public override long GetInt64(int ordinal)
        {
            return Convert.ToInt64(SafeGetProperty(ordinal).Value);
        }

        public override DateTime GetDateTime(int ordinal)
        {
            return Convert.ToDateTime(SafeGetProperty(ordinal).Value);
        }

        public override string GetString(int ordinal)
        {
            return Convert.ToString(SafeGetProperty(ordinal).Value);
        }

        public override decimal GetDecimal(int ordinal)
        {
            return Convert.ToDecimal(SafeGetProperty(ordinal).Value);
        }

        public override double GetDouble(int ordinal)
        {
            return Convert.ToDouble(SafeGetProperty(ordinal).Value);
        }

        public override float GetFloat(int ordinal)
        {
            return Convert.ToSingle(SafeGetProperty(ordinal).Value);
        }

        public override string GetName(int ordinal)
        {
            return _members[ordinal].Name;
        }

        public override int GetValues(object[] values)
        {
            for (int ordinal = 0; ordinal < _members.Count; ++ordinal)
            {
                values[ordinal] = SafeGetProperty(ordinal).Value;
            }
            return _members.Count;
        }

        public override bool IsDBNull(int ordinal)
        {
            return false;
        }

        private RuntimeDefinedParameter SafeGetProperty(int ordinal)
        {
            var name = GetName(ordinal);
            var property = _item[name] ?? new RuntimeDefinedParameter(name, typeof (Object), new Collection<Attribute>());

            return property;
        }

        public override int FieldCount
        {
            get { return _members.Count; }
        }

        public override object this[int ordinal]
        {
            get { return SafeGetProperty(ordinal).Value; }
        }

        public override object this[string name]
        {
            get { return _item[name].Value; }
        }

        public override bool HasRows
        {
            get { return true; }
        }

        public override int GetOrdinal(string name)
        {
            for (int c = 0; c < _members.Count; ++c)
            {
                if (_members[c].Name == name)
                {
                    return c;
                }
            }
            return -1;
        }

        public override string GetDataTypeName(int ordinal)
        {
            return SafeGetProperty(ordinal).ParameterType.FullName;
        }

        public override Type GetFieldType(int ordinal)
        {
            return Type.GetType(SafeGetProperty(ordinal).ParameterType.FullName);
        }

        public override object GetValue(int ordinal)
        {
            return SafeGetProperty(ordinal).Value;
        }

        public override IEnumerator GetEnumerator()
        {
            return null;
        }
    }
}
