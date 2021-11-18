using System;

namespace AutoStar.PrimaryConstructor.Sample
{
    [PrimaryConstructor]
    partial class Service
    {
        private readonly Guid _data;
        private readonly string _name;

        public override string ToString()
        {
            return $"{nameof(Service)}: {_name}\n{_data}";
        }
    }
}