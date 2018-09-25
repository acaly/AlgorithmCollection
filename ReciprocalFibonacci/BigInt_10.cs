using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Numerics;

namespace ReciprocalFibonacci
{
    class BigInt_10
    {
        public IEnumerable<int> Calculate()
        {
            F1 = 1;
            F2 = 2;
            CurrentResult = -1; //2.0 ->3.x
            CurrentEx = 1;
            CurrentTotalEx = 1;
            Entries = new Entry[1000];
            LinkHead = -1;
            EntryCount = EntryEnd = 0;
            while (true)
            {
                var d = MoveNextDigit();
                if (d.HasValue)
                {
                    yield return d.Value;
                }
            }
        }

        public Entry[] Entries;
        public long LinkHead, EntryEnd;
        public long EntryCount;

        public BigInteger CurrentResult;
        public BigInteger CurrentEx;
        public BigInteger CurrentTotalEx;

        public BigInteger F1, F2;

        public int? MoveNextDigit()
        {
            var lastTotalEx = CurrentTotalEx;
            var lastEx = CurrentEx;
            CurrentTotalEx *= 10;
            CurrentEx *= 10;
            CurrentResult *= 10;

            while (F2 < CurrentTotalEx)
            {
                AddEntry(new Entry
                {
                    Denominator = F2,
                    Remainder = lastTotalEx,
                });
                var tmp = F1;
                F1 = F2;
                F2 += tmp;
            }

            for (long i = 0; i < EntryEnd; ++i)
            {
                CurrentResult += StepEntry(i);
            }

            if (CurrentEx - CurrentResult > 10 * (EntryCount + 2))
            {
                var ret = CurrentResult / lastEx;
                CurrentResult -= ret * lastEx;
                CurrentEx /= 10;
                return (int)ret;
            }
            return null;
        }

        public struct Entry
        {
            public BigInteger Denominator;
            public BigInteger Remainder;
            public long LinkedIndex;
        }

        public void AddEntry(Entry e)
        {
            if (LinkHead != -1)
            {
                var nextHead = Entries[LinkHead].LinkedIndex;
                Entries[LinkHead] = e;
                LinkHead = nextHead;
            }
            else if (EntryEnd < Entries.Length)
            {
                Entries[EntryEnd++] = e;
            }
            else
            {
                Entry[] newEntries = new Entry[Entries.Length * 2];
                Array.Copy(Entries, newEntries, Entries.Length);
                Entries = newEntries;
                Entries[EntryEnd++] = e;
            }
            EntryCount++;
        }

        public void RemoveEntry(long index)
        {
            Entries[index].Denominator = 0;
            Entries[index].LinkedIndex = LinkHead;
            LinkHead = index;
            EntryCount--;
        }

        public int StepEntry(long index)
        {
            var e = Entries[index];
            if (e.Denominator.IsZero) return 0;
            var ret = (int)BigInteger.DivRem(e.Remainder * 10, e.Denominator,
                out Entries[index].Remainder);
            if (Entries[index].Remainder.IsZero)
            {
                RemoveEntry(index);
            }
            return ret;
        }
    }
}
