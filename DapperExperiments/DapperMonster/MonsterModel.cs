using System.Collections.Generic;
using Dapper.Contrib.Extensions;

namespace DapperMonster
{
    using System;
    using System.Data.Entity;
    using System.Linq;

    public class MonsterModel : DbContext
    {
        // Your context has been configured to use a 'MonsterModel' connection string from your application's 
        // configuration file (App.config or Web.config). By default, this connection string targets the 
        // 'DapperMonster.MonsterModel' database on your LocalDb instance. 
        // 
        // If you wish to target a different database and/or database provider, modify the 'MonsterModel' 
        // connection string in the application configuration file.
        public MonsterModel()
            : base("name=MonsterModel")
        {
        }

        // Add a DbSet for each entity type that you want to include in your model. For more information 
        // on configuring and using a Code First model, see http://go.microsoft.com/fwlink/?LinkId=390109.

        public virtual DbSet<SimpleMonster> SimpleMonsters { get; set; }
        public virtual DbSet<MonsterAlias> MonsterAliases { get; set; }
    }

    [Table("SimpleMonsters")]
    public class SimpleMonster
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string ScarySound { get; set; }
        public string Habitat { get; set; }

        [ComputedAttribute]
        public virtual List<MonsterAlias> MonsterAliases { get; set; } 
    }

    [Table("MonsterAliases")]
    public class MonsterAlias
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }

        public int SimpleMonsterId { get; set; }

        [ComputedAttribute]
        public virtual SimpleMonster SimpleMonster { get; set; }
    }
}