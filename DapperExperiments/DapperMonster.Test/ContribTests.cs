using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DapperMonster.Test
{
    [TestClass]
    public class ContribTests
    {
        #region Fields and Properties

        private static readonly IDbConnection Db = new SqlConnection(ConfigurationManager.ConnectionStrings["MonsterModel"].ConnectionString);

        #endregion

        #region Utility Methods

        private List<SimpleMonster> _getSimpleMonsters()
        {
            return Db.GetAll<SimpleMonster>().ToList();
        } 

        #endregion

        [TestMethod]
        public void GetAllSimpleMonsters()
        {
            var dynamicMonsters = Db.GetAll<SimpleMonster>().ToList();
            Assert.IsTrue(dynamicMonsters.Any());
        }

        [TestMethod]
        public void GetSimpleMonster()
        {
            var id = _getSimpleMonsters().First().Id;
            var dynamicMonsters = Db.Get<SimpleMonster>(id);
            Assert.IsTrue(dynamicMonsters.Id == id);
        }
        
        [TestMethod]
        public void InsertSimpleMonster()
        {
            var monster = new SimpleMonster
            {
                Name = "Godzilla",
                Habitat = "Japan",
                ScarySound = "undescribable"
            };
            var rv = Db.Insert(monster);
            Assert.IsTrue(rv > 0);
        }

        [TestMethod]
        public void InsertSimpleMonsterList()
        {
            var monster1 = new SimpleMonster
            {
                Name = "Adolf Hitler",
                Habitat = "Nazi Germany",
                ScarySound = "Heil Hitler"
            };
            var monster2 = new SimpleMonster
            {
                Name = "Paul Pot",
                Habitat = "Asia",
                ScarySound = "not sure"
            };
            var monster3 = new SimpleMonster
            {
                Name = "Joseph Stalin",
                Habitat = "Moscow",
                ScarySound = "I pick you..."
            };
            var monsters = new List<SimpleMonster>();
            monsters.Add(monster1);
            monsters.Add(monster2);
            monsters.Add(monster3);

            var rv = Db.Insert(monsters);
            Assert.IsTrue(rv > 0);
        }

        [TestMethod]
        public void UpdateSimpleMonster()
        {
            var monster =_getSimpleMonsters().First();
            monster.ScarySound = "My mother is coming to stay with us for a month";
            Db.Update(monster);
        }
        
        [TestMethod]
        public void UpdateManySimpleMonsters()
        {
            var monsters = _getSimpleMonsters().Take(2).ToList();
            foreach (var monster in monsters)
            {
                monster.ScarySound = "My mother is coming to stay with us for a month";
            }
            Db.Update(monsters);
        }

        [TestMethod]
        public void DeleteSimpleMonster()
        {
            var monster = _getSimpleMonsters().First();
            Db.Delete(monster);
        }

        [TestMethod]
        public void DeleteSimpleMonsters()
        {
            var monsters = _getSimpleMonsters().Take(2).ToList();
            Db.Delete(monsters);
        }
        [TestMethod]
        public void DeleteAllSimpleMonsters()
        {
            Db.DeleteAll<SimpleMonster>();
        }

    }
}
