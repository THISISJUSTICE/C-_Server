using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server.DB;

namespace Server
{
    public static class Extensions
    {
        // 확장 메소드
        public static bool SaveChangesEx(this AppDbContext db) {
            try
            {
                db.SaveChanges();
                return true;
            }
            catch {
                return false;
            }
        }
    }
}
