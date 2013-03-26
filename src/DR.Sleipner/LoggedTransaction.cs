using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DR.Sleipner
{
    public class LoggedTransaction
    {
        public string Event;
        public DateTime Started;
        public DateTime Ended;
        public IList<string> Notes;

        public LoggedTransaction(string @event)
        {
            Event = @event;
            Started = DateTime.Now;

            Notes = new List<string>();
        }

        public void AddNote(string note)
        {
            Notes.Add(note);
        }
    }
}
