using DevExpress.Xpo;
using OpenAI.Utilities.FunctionCalling;
using System.Text;

namespace AI.Labs.Module.BusinessObjects.Contexts
{
    public class SQLHelper
    {
        static SQLHelper()
        {

        }

        //public static Session Session;
        [FunctionDescription("传入sql语句,返回执行结果,数据库是Microsoft SQLServer.")]
        public string Execute(string sql)
        {
            var layer = XpoDefault.GetDataLayer("Integrated Security=SSPI;Pooling=false;Data Source=.;Initial Catalog=ERP", DevExpress.Xpo.DB.AutoCreateOption.SchemaAlreadyExists);
            var Session = new Session(layer);
            
            var str = new StringBuilder();
            var query = Session.ExecuteQuery(sql);
            foreach (var item in query.ResultSet)
            {
                foreach (var r in item.Rows)
                {
                    foreach (var v in r.Values)
                    {
                        str.Append($"'{v?.ToString()}',");
                    }
                    str.Append("\n");
                }
            }
            return str.ToString();
        }
    }
}
