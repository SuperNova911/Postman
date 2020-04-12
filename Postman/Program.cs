using System;

namespace Postman
{
    class Program
    {
        static void Main(string[] args)
        {


            DatabaseManager.Instance.Close();
        }
    }
}
