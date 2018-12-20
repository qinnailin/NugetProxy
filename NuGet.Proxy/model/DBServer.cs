using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SqlSugar;

namespace NuGet.Proxy.model
{
    public class DBServer
    {
        private SqlSugarClient db;
        public DBServer(string connstring)
        {
            db = new SqlSugarClient(new ConnectionConfig {
                ConnectionString = connstring,
                DbType=DbType.SqlServer,
                IsAutoCloseConnection=true
            });
        }

        public ODataPackage Get(string id,string version)
        {
            return db.Queryable<ODataPackage>().Where(w =>w.Id==id&&w.Version==version).First() ;
        }

        public void Save(ODataPackage bean)
        {
            var bo = Get(bean.Id, bean.Version);
            if (bo == null)
            {
                db.Insertable(bean).ExecuteCommand();
            }
            else
            {
                bo.IsDowning = bean.IsDowning;
                db.Updateable(bean).ExecuteCommand();
            }
        }
    }
}
