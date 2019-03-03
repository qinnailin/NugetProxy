using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlueFox.Log;
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
                DbType=DbType.MySql,
                IsAutoCloseConnection=true
            });
            db.Ado.IsEnableLogEvent = true;

            db.Ado.LogEventStarting = (sql, pars) =>
            {
                LogService.Error(sql + "\r\n" + db.Utilities.SerializeObject(pars.ToDictionary(it => it.ParameterName, it => it.Value)));
            };
        }

        public ODataPackage Get(string id,string version)
        {
            //var res = db.Queryable<ODataPackage>().Where(w=> w.NormalizedVersion == version).ToList();
            return db.Queryable<ODataPackage>().Where(w =>w.Id==id&&(w.NormalizedVersion == version||w.Version==version)).First() ;
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
