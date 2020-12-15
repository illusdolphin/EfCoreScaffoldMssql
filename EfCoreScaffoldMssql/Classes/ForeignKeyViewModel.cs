namespace EfCoreScaffoldMssql.Classes
{
    public class ForeignKeyViewModel: FkDefinition
    {
        public string PropertyName { get; set; }

        public string InversePropertyName { get; set; }

        public bool IsOneToOne { get; set; }
        public bool IsManyToMany { get; set; }

        public string InverseEntityName { get; set; }

        public bool IsOptional { get; set; }
    }
}