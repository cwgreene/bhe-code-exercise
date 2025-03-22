using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;
using System.Security.Cryptography.X509Certificates;
using Microsoft.VisualBasic;

namespace Sieve;

public interface ISieve
{
    long NthPrime(long n);
}

public class SieveImplementation : ISieve
{
    private long estimate_size(long n) {
        // for small n, just go with 2000,
        // bounds for the next part kick in once x > 229
        // and we know that when pi(x) = 229, x > 229.

        if ( n < 229) {
            return 2000;
        }

        // https://en.wikipedia.org/wiki/Prime-counting_function
        //
        // number of primes less than x is pi(x)
        // which is approximately x/log(x)
        // we want pi(x) = n so that n primes
        // pop out.

        // x/log(x) = n is transcendental, so
        // won't have a closed form solution.

        // We could find the exact value in the 
        // integers by bisection, but since this is
        // just an estimate anyhow, we might as well
        // just double things up and not worry too much about precision.
        // The important thing is that we're not too small (which would truncate
        // our primes)

        // We know that pi(x) < x so if we want to find xn such that pi(xn) = n
        // so we start with the guess n < xn;
        var guess = n; 

        // The difference between li(x) and pi(x) is less than x (for x > 229),
        // so |li(xn)-pi(xn)| = |li(xn) - n| will be never more than twice n.
        // The bound is much tighter, but this memory allocation works for all
        // test cases.
        while (guess / Math.Log(guess) < 2*n) {
            guess = 2*guess;
        }
        return guess;
    }

    private void remove_multiples(bool[] sieve, long cur_p, long start) {
        // Remove multiples
        long m = start / cur_p;
        if (cur_p * m < start) {
            m += 1;
        }
        while (cur_p*m < sieve.Length + start ) {
            sieve[cur_p*m-start] = false;
            m += 1;
        }
    }

    // Returns
    // List<long> prime_list
    // long prime_count
    // long max_p
    private (List<long>, long, long) perform_sieve(bool[] sieve,
        List<long> prime_list,
        long nth_prime, 
        long prime_count = 0,
        long start = 2)
    {
        // start of sieve is 2
        long j = 0;
        long cur_p = 2;
        long cutoff = sieve.Length; // This is compatible with extended sieve

        foreach (var p in prime_list) {
            remove_multiples(sieve, p, start);
        }

        while (j < sieve.Length ) {
            if (sieve[j] == true) {
                cur_p = j + start;
                if ( prime_list.Count <= cutoff) {
                    prime_list.Add(cur_p);
                }
                prime_count += 1;
                if ( nth_prime + 1 == prime_count) {
                    return (prime_list, prime_count, cur_p);
                }
                // Sieve is guaranteed to be less than 2**32 in the
                // entry function, so double math here is accurate.
                if ( j < Math.Ceiling(Math.Sqrt(sieve.Length)) ) {
                    remove_multiples(sieve, cur_p, start);
                }
            }
            j++;
        }
        return (prime_list, prime_count, cur_p);
    }

    private bool[] init_sieve(long n) {
        bool[] sieve = new bool[n];
        for( long i = 0; i < n; i++) {
            sieve[i] = true;
        }
        return sieve;
    }

    public long NthPrime(long n)
    {
        if (n < 0) {
            throw new ArgumentException("n must be a positive number.");
        }

        var max_array_size = estimate_size(n);
        var delta = (long)Math.Ceiling(Math.Sqrt(max_array_size));
        if (delta >= Math.Pow(2, 31)  ) {
            // We're going to allocate 2 gigabytes at this point (bools are a), and well
            // that's a little silly (and apparently bigger than what csharp can do).
            // We should switch to a paging method at that point.
            // Another cut off is about 2^53, when we start running into
            // floating point rounding issues of our doubles. To fix that, we should
            // switch to bigints or something.
            throw new ArgumentException("Argument is too large");
        }
        var prime_list = new List<long>();

        if (n < prime_list.Count) {
            return prime_list[(int)n];
        }

        long prime_count = 0;
        long max_p = 2;
        long start = 2;
        while (start <= delta*delta) {
            var sieve = init_sieve(delta);
            (prime_list, prime_count, max_p) = perform_sieve(sieve, prime_list, nth_prime: n, prime_count: prime_count, start: start);
            if (prime_count == n + 1) {
                return max_p;
            }
            start += sieve.Length;
        }
        return max_p;
    }
}