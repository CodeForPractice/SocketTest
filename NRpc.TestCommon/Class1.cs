using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRpc.TestCommon
{
    public class User : IUser
    {
        public int GetAge()
        {
            return 10;
        }

        public int GetAge(int age)
        {
            return age;
        }

        public bool SetAget(int age)
        {
            return true;
        }
    }

    public interface IUser
    {
        int GetAge();

        int GetAge(int age);

        bool SetAget(int age);
    }
}
