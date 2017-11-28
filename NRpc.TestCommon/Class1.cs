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

        public Task AsyncAction()
        {
            return Task.Delay(1);
        }

        public Task<int> GetAgeAsync()
        {
            return Task.FromResult(100);
        }

        public void SetMessage()
        {

        }

        public TestModel Model()
        {

            return new TestModel
            {
                Age = 10,
                Message = "1234567"
            };
        }

        public TestModel GetNull()
        {
            return null;
        }

        public Task<TestModel> GetNullAsync()
        {
            return Task.FromResult(Model());
        }
    }

    public interface IUser
    {
        int GetAge();

        int GetAge(int age);

        bool SetAget(int age);

        Task AsyncAction();

        Task<int> GetAgeAsync();

        void SetMessage();

        TestModel Model();

        TestModel GetNull();

        Task<TestModel> GetNullAsync();
    }
    [Serializable]
    public class TestModel
    {
        public int Age { get; set; }

        public string Message { get; set; }

        public override string ToString()
        {
            return $"AGE:{Age},Message:{Message}";
        }


    }
}
