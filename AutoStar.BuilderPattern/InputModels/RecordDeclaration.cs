﻿using System.Collections.Generic;

namespace AutoStar.BuilderPattern
{
    public class RecordDeclaration
    {
        public RecordDeclaration(string name, IReadOnlyCollection<RecordProperty> recordProperties)
        {
            Name = name;
            RecordProperties = recordProperties;
        }

        public string Name { get; }
        public IReadOnlyCollection<RecordProperty> RecordProperties { get; }
    }
}