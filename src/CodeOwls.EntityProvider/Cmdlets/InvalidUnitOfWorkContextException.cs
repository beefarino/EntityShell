using System;
using System.Runtime.Serialization;

namespace CodeOwls.EntityProvider.Cmdlets
{
    [Serializable]
    public class InvalidUnitOfWorkContextException : ApplicationException
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public InvalidUnitOfWorkContextException()
        {
        }

        public InvalidUnitOfWorkContextException(string message) : base(message)
        {
        }

        public InvalidUnitOfWorkContextException(string message, Exception inner) : base(message, inner)
        {
        }

        protected InvalidUnitOfWorkContextException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}