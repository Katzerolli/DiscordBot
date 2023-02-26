using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Functions
{
    public class Lottery<T>
    {
        public class Ticket
        {
            public T Key { get; private set; }
            public double Weight { get; private set; }
            public Ticket(T key, double weight)
            {
                this.Key = key;
                this.Weight = weight;
            }
        }
        List<Ticket> tickets = new List<Ticket>();
        static Random rand = new Random();
        public void Add(T key, double weight)
        {
            tickets.Add(new Ticket(key, weight));
        }
        public Ticket Draw(bool removeWinner)
        {
            double r = rand.NextDouble() * tickets.Sum(a => a.Weight);
            double min = 0;
            double max = 0;
            Ticket winner = null;
            foreach (var ticket in tickets)
            {
                max += ticket.Weight;
                //-----------
                if (min <= r && r < max)
                {
                    winner = ticket;
                    break;
                }
                //-----------
                min = max;
            }
            if (winner == null) throw new Exception();
            if (removeWinner) tickets.Remove(winner);
            return winner;
        }
    }
}
