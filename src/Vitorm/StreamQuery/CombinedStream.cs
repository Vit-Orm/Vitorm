using System;
using System.Collections.Generic;
using System.Linq;

using Vit.Linq.ExpressionTree.ComponentModel;

namespace Vitorm.StreamQuery
{
    /* //sql
    select u.id, u.name, u.birth,u.fatherId ,u.motherId,    father.name,  mother.name
    from `User` u
    inner join `User` father on u.fatherId = father.id 
    left join `User` mother on u.motherId = mother.id
    where u.id>1
    limit 1,5;
     */

    /* //linq
value(Vit.Linq.Converter.OrderedQueryable`1[Vit.Linq.MsTest.Converter.Join_Test+User])
.SelectMany(
     user => value(Vit.Linq.MsTest.Converter.Join_Test+<>c__DisplayClass0_1).users
             .Where(father => (Convert(father.id, Nullable`1) == user.fatherId)).DefaultIfEmpty(),
     (user, father) => new <>f__AnonymousType4`2(user = user, father = father)
 ).SelectMany(
     item => value(Vit.Linq.MsTest.Converter.Join_Test+<>c__DisplayClass0_1).users
                 .Where(mother => (Convert(mother.id, Nullable`1) == item.user.fatherId)).DefaultIfEmpty(),
     (item, mother) => new <>f__AnonymousType5`3(user = item.user, father = item.father, mother = mother)
 )
.Skip().Take().Select()
     */



    public class CombinedStream : IStream
    {

        public CombinedStream(string alias)
        {
            this.alias = alias;
        }

        /// <summary>
        /// default is ToList, could be :  Count | First | FirstOrDefault | Last | LastOrDefault | TotalCount
        /// </summary>
        public string method { get; set; }


        public string alias { get; protected set; }

        // ExpressionNode_New   new { c = a , d = b }
        public SelectedFields select { get; set; }
        public bool? distinct;


        public IStream source;


        public List<StreamToJoin> joins { get; set; }
        public ExpressionNode GetSelectedFields(Type entityType)
        {
            var parameterValue = select?.fields as ExpressionNode;
            if (parameterValue == null && joins?.Any() != true)
            {
                parameterValue = ExpressionNode_RenameableMember.Member(stream: source, entityType);
            }
            return parameterValue;
        }



        //  a1.id==b2.id
        public ExpressionNode where { get; set; }



        // ExpressionNode_New     new {  fatherId = left.fatherId, motherId = left.motherId }
        // ExpressionNode_Member  left.fatherId
        public ExpressionNode groupByFields;

        //  left.fatherId >2 && left.Count()>2 && left.Max(m=>m.id) >2
        public ExpressionNode having { get; set; }




        //  a1.id, b2.id
        public List<OrderField> orders { get; set; }


        public int? skip { get; set; }
        public int? take { get; set; }


        public bool isJoinedStream => joins?.Any() == true;
        public bool isGroupedStream => groupByFields != null;
    }
}
