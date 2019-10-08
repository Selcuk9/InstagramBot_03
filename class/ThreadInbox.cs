using InstagramApiSharp.Classes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstagramBot_03
{
   class ThreadInbox
{
    public class Pair_Message_User
    {
        public String text;
        public long user_id;
        public String username;
        public Pair_Message_User(InstaDirectInboxItem item)
        {
            this.text = item.Text;
            this.user_id = item.UserId;
        }
    }
    public String thread_id;
    String username;
    public String UserName
    {
        get
        {
            return username;
        }
        set
        {
            username = value;
        }
    }
    public List<Pair_Message_User> messages = new List<Pair_Message_User>();

    public ThreadInbox(String thread_id)
    {
        this.thread_id = thread_id;
    }
    public int AddMessages(List<InstaDirectInboxItem> items)
    {
        int res = 0;
        foreach (var item in items)
        {
            if (item.ItemType == InstaDirectThreadItemType.Text)
            {
                var message = new Pair_Message_User(item);
                this.messages.Add(message);
                res++;
            }
        }
        return res;
    }



}
}
