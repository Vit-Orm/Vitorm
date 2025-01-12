namespace Vit.Linq.ExpressionNodes.ExpressionNodesTest
{

    public abstract partial class ExpressionTester
    {
        static bool canCalculate = true;

        [System.ComponentModel.DataAnnotations.Schema.Table("User2")]
        public class User : Vitorm.MsTest.User
        {
        }


        public static List<User> GetSourceData()
        {
            int count = 1000;

            var Now = DateTime.Now;
            var list = new List<User>(count);
            for (int i = 1; i < count; i++)
            {
                list.Add(new User
                {
                    id = i,
                    name = "name" + i,
                    birth = Now.AddSeconds(i),
                    fatherId = i >= 2 ? i >> 1 : null,
                    motherId = i >= 2 ? (i >> 1) + 1 : null,
                });
            }
            return list;
        }


    }
}
