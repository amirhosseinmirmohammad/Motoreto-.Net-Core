using System;

namespace Domain
{
    public class UserTransactions
    {
        /// <summary>
        /// this class is using for detecting the user transactions in a while
        /// </summary>
        public UserTransactions() { }

        public long id { get; set; }

        public string userId { get; set; }

        public DateTime visitDateTime { get; set; }
    }
}
