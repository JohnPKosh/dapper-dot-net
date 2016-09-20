using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DapperMonster.Test
{
    [TestClass]
    public class BasicTests
    {
        #region Fields and Properties

        private static readonly IDbConnection Db = new SqlConnection(ConfigurationManager.ConnectionStrings["MonsterModel"].ConnectionString);

        public string InsertMonsterSql => @"INSERT INTO [dbo].[SimpleMonsters] ([Name]) VALUES('Baloo')";
        public string DeleteAllMonstersSql => @"DELETE FROM [dbo].[SimpleMonsters]";

        public string DropInsertSimpleMonsterProcedure
        {
            get
            {
                // ReSharper disable ConvertPropertyToExpressionBody
               return 
@"IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_Insert_SimpleMonster]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_Insert_SimpleMonster]";
                // ReSharper restore ConvertPropertyToExpressionBody
            }
        }

        public string CreateInsertSimpleMonsterProcedure
        {
            // ReSharper disable ConvertPropertyToExpressionBody
            get { return 
@"CREATE PROCEDURE usp_Insert_SimpleMonster 
	@Name NVARCHAR(MAX)
AS
BEGIN
INSERT INTO [dbo].[SimpleMonsters]
           ([Name])
     VALUES
           (@Name)
END
";
                // ReSharper restore ConvertPropertyToExpressionBody
            }
        }

        #endregion

        #region Simple Monster Basic Tests

        [TestMethod]
        public void EFDatabaseInitialization_AddFirstMonsters()
        {
            using (var model = new MonsterModel())
            {
                model.SimpleMonsters.Add(new SimpleMonster() { Name = "Cookie Monster", Habitat = "Garbage Can", ScarySound = "yum!"});
                model.SimpleMonsters.Add(new SimpleMonster() { Name = "Flint", Habitat = "Shadows", ScarySound = "ooooooo!" });
                model.SimpleMonsters.Add(new SimpleMonster() { Name = "The Grinch", Habitat = "Garbage Dump", ScarySound = "sooo" });
                model.SimpleMonsters.Add(new SimpleMonster() { Name = "Mike Wazowski", Habitat = "Opera House", ScarySound = "Blah"});
                model.SimpleMonsters.Add(new SimpleMonster() { Name = "James P. Sullivan", Habitat = "Hawaii", ScarySound = "blah blah", MonsterAliases = new List<MonsterAlias>() { new MonsterAlias() { Name = "Sully" } } });
                model.SimpleMonsters.Add(new SimpleMonster() { Name = "Celia Mae", MonsterAliases = new List<MonsterAlias>() {new MonsterAlias() {Name = "Celia"} } });
                model.SimpleMonsters.Add(new SimpleMonster() { Name = "Boo" });
                model.SimpleMonsters.Add(new SimpleMonster() { Name = "Randall" });
                model.SimpleMonsters.Add(new SimpleMonster() { Name = "Roz" });
                model.SimpleMonsters.Add(new SimpleMonster() { Name = "Henry J. Waternoose" });
                model.SaveChanges();
            }
        }

        [TestMethod]
        public void SelectDynamicMonsters()
        {
            var dynamicMonsters = Db.Query("SELECT * FROM SimpleMonsters");
            Assert.IsTrue(dynamicMonsters.Any());
        }

        [TestMethod]
        public void SelectDynamicMonstersUnbuffered()
        {
            var dynamicMonsters = Db.Query("SELECT * FROM SimpleMonsters", buffered:false);
            Assert.IsTrue(dynamicMonsters.Any());
        }

        [TestMethod]
        public void SelectMonsters()
        {
            var typedMonsters = Db.Query<SimpleMonster>("SELECT * FROM SimpleMonsters");
            Assert.IsTrue(typedMonsters.Any());
        }
        
        [TestMethod]
        public void SelectMonstersWithParameter()
        {
            var typedMonsters = Db.Query<SimpleMonster>("SELECT * FROM SimpleMonsters WHERE [Name] = @Name", new {Name= "Flint" });
            Assert.IsTrue(typedMonsters.Any());
        }

        [TestMethod]
        public void SelectMonstersWithMultipleParameter()
        {
            var typedMonsters = Db.Query<SimpleMonster>("SELECT * FROM SimpleMonsters WHERE [Name] = @Name OR [Habitat] IN @Habitats", new { Name = "Flint", Habitats = new[] { "Hawaii", "Garbage Can" } }).ToList();
            Assert.IsTrue(typedMonsters.Any());
            Assert.IsTrue(typedMonsters.Count == 3); // James P. Sullivan, Cookie Monster, and Flint
        }
        
        [TestMethod]
        public void SelectMultimappedQuery()
        {
            var typedMonsters = Db.Query<MonsterAlias, SimpleMonster, MonsterAlias>("SELECT * FROM MonsterAlias child, SimpleMonsters parent WHERE parent.Id = child.SimpleMonsterId AND parent.Name = 'James P. Sullivan'", (alias, monster) =>
            {
                alias.SimpleMonster = monster; return alias;
            });
            Assert.IsTrue(typedMonsters.Any());
        }

        [TestMethod]
        public void SelectMultipleResults()
        {
            var sql =
@"
SELECT * FROM SimpleMonsters WHERE [Name] = @Name;
SELECT * FROM MonsterAlias;";

            using (var multi = Db.QueryMultiple(sql, new { Name = "Flint" }))
            {
                var monsters = multi.Read<SimpleMonster>().ToList();
                var aliases = multi.Read<MonsterAlias>().ToList();

                Assert.IsTrue(monsters.Any());
                Assert.IsTrue(aliases.Any());
            }
        }

        [TestMethod]
        public void ExecuteInsertMonster()
        {
            var typedMonsters = Db.Execute(InsertMonsterSql);
            Assert.IsTrue(typedMonsters == 1);
        }

        [TestMethod]
        public void ExecuteDeleteAllMonsters()
        {
            Db.Execute(DeleteAllMonstersSql);
        }

        [TestMethod]
        public void ExecuteCreateInsertSimpleMonsterProcedure()
        {
            Db.Execute(DropInsertSimpleMonsterProcedure);
            Db.Execute(CreateInsertSimpleMonsterProcedure);
        }

        [TestMethod]
        public void ExecuteInsertMonsterProcedure()
        {
            var parameter = new DynamicParameters();
            parameter.Add("@Name", "Scooby Doo");
            Db.Execute("usp_Insert_SimpleMonster", parameter, commandType: CommandType.StoredProcedure);
        }

        [TestMethod]
        public void ExecuteUpdateMonster()
        {
            var result = Db.Execute(@"UPDATE [dbo].[SimpleMonsters] SET [ScarySound] = 'Shagggggy', [Habitat] = 'Van' WHERE [Name] = 'Scooby Doo'");
            Assert.IsTrue(result == 1);
        }

        #endregion
    }
}
