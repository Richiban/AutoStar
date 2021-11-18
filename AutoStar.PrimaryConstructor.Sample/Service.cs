using System;

namespace AutoStar.PrimaryConstructor.Sample
{
    [PrimaryConstructor]
    internal partial class Service
    {
        private readonly Guid _data;
        private readonly string _name;

        public override string ToString() => $"{nameof(Service)}: {_name}\n{_data}";
    }
}