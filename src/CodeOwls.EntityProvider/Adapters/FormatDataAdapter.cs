using System;
using System.Collections.Generic;

namespace CodeOwls.EntityProvider.Adapters
{
    class FormatDataAdapter
    {
        private readonly List<Type> _pocoTypes;

        public FormatDataAdapter(List<Type> pocoTypes)
        {
            _pocoTypes = pocoTypes;
        }
    }
}
