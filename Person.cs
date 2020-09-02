using System;
using System.Collections.Generic;
using System.Text;

namespace queueitems.Model
{
    class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public List<Connection> Connections { get; set; }

    }

    class Connection
    {
        public string RelatedPerson { get; set; }
        public string Relationship { get; set; }
    }

}
