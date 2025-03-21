using System.Reflection.Metadata;
using System.Security.Cryptography.X509Certificates;

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

    private long perform_sieve(bool[] sieve, long k)
    {
        // start of sieve is 2
        long j = 0;
        long prime_count = 0;
        long cur_p = 2;

        while (j < sieve.Length ) {
            if (sieve[j] == true) {
                cur_p = j + 2;
                ///Console.WriteLine($"{cur_p}");
                if (k == prime_count) {
                    return cur_p;
                }
                prime_count += 1;

                // Sieve is guaranteed to be less than 2**32 in the
                // entry function, so double math here is accurate.
                if ( j < Math.Ceiling(Math.Sqrt(sieve.Length)) ) {
                    // Remove multiples
                    int m = 2;
                    while (cur_p*m < sieve.Length ) {
                        sieve[cur_p*m-2] = false;
                        m += 1;
                    }
                }
            }
            j++;
        }
        return cur_p;
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
        Console.WriteLine(max_array_size);
        if (max_array_size >= Math.Pow(2, 31)  ) {
            // We're going to allocate 2 gigabytes at this point (bools are a), and well
            // that's a little silly (and apparently bigger than what csharp can do).
            // We should switch to a paging method at that point.
            // Another cut off is about 2^53, when we start running into
            // floating point rounding issues of our doubles. To fix that, we should
            // switch to bigints or something.
            throw new ArgumentException("Argument is too large");
        }
        var sieve = init_sieve(max_array_size);
        var nth_prime = perform_sieve(sieve, n);
        return nth_prime;
    }
}