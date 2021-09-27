using System;
using System.Collections.Generic;
using System.Globalization;
using EnsureThat;
using Microsoft.Extensions.Primitives;

namespace AutoFixture.Extensions.Tests
{
    /// <summary>
    /// Struct based on Microsoft.AspNetCore.Http.HostString
    /// </summary>
    public readonly struct StructChild : IEquatable<StructChild>
    {
        public StructChild(string value)
        {
            _value = value;
        }

        public StructChild(string host, int port)
        {
            Ensure.That(host).IsNotNullOrEmpty();
            Ensure.That(port).IsGt(0);

            int index;
            if (host.IndexOf('[') == -1
                && (index = host.IndexOf(':')) >= 0
                && index < host.Length - 1
                && host.IndexOf(':', index + 1) >= 0)
            {
                // IPv6 without brackets ::1 is the only type of host with 2 or more colons
                host = $"[{host}]";
            }

            _value = host + ":" + port.ToString(CultureInfo.InvariantCulture);
        }

        #region Properties

        private readonly string _value;

        /// <summary>
        /// Returns the original value from the constructor.
        /// </summary>
        public string Value => _value;

        /// <summary>
        /// Returns true if the host is set.
        /// </summary>
        public bool HasValue => !string.IsNullOrEmpty(Value);

        /// <summary>
        /// Returns the value of the host part of the value. The port is removed if it was present.
        /// IPv6 addresses will have brackets added if they are missing.
        /// </summary>
        /// <returns>The host portion of the value.</returns>
        public string Host
        {
            get
            {
                GetParts(_value, out var host, out var port);
                return host.ToString();
            }
        }

        /// <summary>
        /// Returns the value of the port part of the host, or <value>null</value> if none is found.
        /// </summary>
        /// <returns>The port portion of the value.</returns>
        public int? Port
        {
            get
            {
                GetParts(_value, out var host, out var port);

                if (!StringSegment.IsNullOrEmpty(port)
                    && int.TryParse(port.AsSpan(), NumberStyles.None, CultureInfo.InvariantCulture, out var p))
                {
                    return p;
                }

                return null;
            }
        }

        #endregion

        #region Public

        /// <summary>
        /// Returns the value as normalized by ToUriComponent().
        /// </summary>
        /// <returns>The value as normalized by <see cref="ToUriComponent"/>.</returns>
        public override string ToString()
        {
            return ToUriComponent();
        }

        /// <summary>
        /// Returns the value properly formatted and encoded for use in a URI in a HTTP header.
        /// Any Unicode is converted to punycode. IPv6 addresses will have brackets added if they are missing.
        /// </summary>
        /// <returns>The <see cref="StructChild"/> value formatted for use in a URI or HTTP header.</returns>
        public string ToUriComponent()
        {
            if (string.IsNullOrEmpty(_value))
            {
                return string.Empty;
            }

            int i;
            for (i = 0; i < _value.Length; ++i)
            {
                if (!HostStringHelper.IsSafeHostStringChar(_value[i]))
                {
                    break;
                }
            }

            if (i != _value.Length)
            {
                GetParts(_value, out var host, out var port);

                var mapping = new IdnMapping();
                var encoded = mapping.GetAscii(host.Buffer, host.Offset, host.Length);

                return StringSegment.IsNullOrEmpty(port)
                    ? encoded
                    : string.Concat(encoded, ":", port.ToString());
            }

            return _value;
        }

        /// <summary>
        /// Creates a new HostString from the given URI component.
        /// Any punycode will be converted to Unicode.
        /// </summary>
        /// <param name="uriComponent">The URI component string to create a <see cref="StructChild"/> from.</param>
        /// <returns>The <see cref="StructChild"/> that was created.</returns>
        public static StructChild FromUriComponent(string uriComponent)
        {
            if (!string.IsNullOrEmpty(uriComponent))
            {
                int index;
                if (uriComponent.IndexOf('[') >= 0)
                {
                    // IPv6 in brackets [::1], maybe with port
                }
                else if ((index = uriComponent.IndexOf(':')) >= 0
                    && index < uriComponent.Length - 1
                    && uriComponent.IndexOf(':', index + 1) >= 0)
                {
                    // IPv6 without brackets ::1 is the only type of host with 2 or more colons
                }
                else if (uriComponent.IndexOf("xn--", StringComparison.Ordinal) >= 0)
                {
                    // Contains punycode
                    if (index >= 0)
                    {
                        // Has a port
                        string port = uriComponent.Substring(index);
                        var mapping = new IdnMapping();
                        uriComponent = mapping.GetUnicode(uriComponent, 0, index) + port;
                    }
                    else
                    {
                        var mapping = new IdnMapping();
                        uriComponent = mapping.GetUnicode(uriComponent);
                    }
                }
            }
            return new StructChild(uriComponent);
        }

        /// <summary>
        /// Creates a new HostString from the host and port of the give Uri instance.
        /// Punycode will be converted to Unicode.
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> to create a <see cref="HostString"/> from.</param>
        /// <returns>The <see cref="HostString"/> that was created.</returns>
        public static StructChild FromUriComponent(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            return new StructChild(uri.GetComponents(
                UriComponents.NormalizedHost | // Always convert punycode to Unicode.
                UriComponents.HostAndPort, UriFormat.Unescaped));
        }

        /// <summary>
        /// Matches the host portion of a host header value against a list of patterns.
        /// The host may be the encoded punycode or decoded unicode form so long as the pattern
        /// uses the same format.
        /// </summary>
        /// <param name="value">Host header value with or without a port.</param>
        /// <param name="patterns">A set of pattern to match, without ports.</param>
        /// <remarks>
        /// The port on the given value is ignored. The patterns should not have ports.
        /// The patterns may be exact matches like "example.com", a top level wildcard "*"
        /// that matches all hosts, or a sub-domain wildcard like "*.example.com" that matches
        /// "abc.example.com:443" but not "example.com:443".
        /// Matching is case insensitive.
        /// </remarks>
        /// <returns><see langword="true" /> if <paramref name="value"/> matches any of the patterns.</returns>
        public static bool MatchesAny(StringSegment value, IList<StringSegment> patterns)
        {
            // Drop the port
            GetParts(value, out var host, out var port);

            for (var i = 0; i < port.Length; i++)
            {
                if (port[i] < '0' || '9' < port[i])
                {
                    throw new FormatException($"The given host value '{value}' has a malformed port.");
                }
            }

            var count = patterns.Count;
            for (var i = 0; i < count; i++)
            {
                var pattern = patterns[i];

                if (pattern == "*")
                {
                    return true;
                }

                if (StringSegment.Equals(pattern, host, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                // Sub-domain wildcards: *.example.com
                if (pattern.StartsWith("*.", StringComparison.Ordinal) && host.Length >= pattern.Length)
                {
                    // .example.com
                    var allowedRoot = pattern.Subsegment(1);

                    var hostRoot = host.Subsegment(host.Length - allowedRoot.Length);
                    if (hostRoot.Equals(allowedRoot, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion

        #region Private

        /// <summary>
        /// Parses the current value. IPv6 addresses will have brackets added if they are missing.
        /// </summary>
        /// <param name="value">The value to get the parts of.</param>
        /// <param name="host">The portion of the <paramref name="value"/> which represents the host.</param>
        /// <param name="port">The portion of the <paramref name="value"/> which represents the port.</param>
        private static void GetParts(StringSegment value, out StringSegment host, out StringSegment port)
        {
            int index;
            port = null;
            host = null;

            if (StringSegment.IsNullOrEmpty(value))
            {
                return;
            }

            if ((index = value.IndexOf(']')) >= 0)
            {
                // IPv6 in brackets [::1], maybe with port
                host = value.Subsegment(0, index + 1);
                // Is there a colon and at least one character?
                if (index + 2 < value.Length && value[index + 1] == ':')
                {
                    port = value.Subsegment(index + 2);
                }
            }
            else if ((index = value.IndexOf(':')) >= 0
                     && index < value.Length - 1
                     && value.IndexOf(':', index + 1) >= 0)
            {
                // IPv6 without brackets ::1 is the only type of host with 2 or more colons
                host = $"[{value}]";
                port = null;
            }
            else if (index >= 0)
            {
                // Has a port
                host = value.Subsegment(0, index);
                port = value.Subsegment(index + 1);
            }
            else
            {
                host = value;
                port = null;
            }
        }

        #endregion

        #region Equality

        /// <summary>
        /// Compares the equality of the Value property, ignoring case.
        /// </summary>
        /// <param name="other">The <see cref="StructChild"/> to compare against.</param>
        /// <returns><see langword="true" /> if they have the same value.</returns>
        public bool Equals(StructChild other)
        {
            if (!HasValue && !other.HasValue)
            {
                return true;
            }
            return string.Equals(_value, other._value, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Compares against the given object only if it is a HostString.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare against.</param>
        /// <returns><see langword="true" /> if they have the same value.</returns>
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return !HasValue;
            }
            return obj is StructChild && Equals((StructChild)obj);
        }

        /// <summary>
        /// Gets a hash code for the value.
        /// </summary>
        /// <returns>The hash code as an <see cref="int"/>.</returns>
        public override int GetHashCode()
        {
            return (HasValue ? StringComparer.OrdinalIgnoreCase.GetHashCode(_value) : 0);
        }

        /// <summary>
        /// Compares the two instances for equality.
        /// </summary>
        /// <param name="left">The left parameter.</param>
        /// <param name="right">The right parameter.</param>
        /// <returns><see langword="true" /> if both <see cref="StructChild"/>'s have the same value.</returns>
        public static bool operator ==(StructChild left, StructChild right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares the two instances for inequality.
        /// </summary>
        /// <param name="left">The left parameter.</param>
        /// <param name="right">The right parameter.</param>
        /// <returns><see langword="true" /> if both <see cref="StructChild"/>'s values are not equal.</returns>
        public static bool operator !=(StructChild left, StructChild right)
        {
            return !left.Equals(right);
        }

        #endregion
    }

    /// <summary>
    /// Helper class for HostString
    /// </summary>
    internal static class HostStringHelper
    {
        // Allowed Characters:
        // A-Z, a-z, 0-9, ., 
        // -, %, [, ], : 
        // Above for IPV6
        private static readonly bool[] SafeHostStringChars = {
            false, false, false, false, false, false, false, false,     // 0x00 - 0x07
            false, false, false, false, false, false, false, false,     // 0x08 - 0x0F
            false, false, false, false, false, false, false, false,     // 0x10 - 0x17
            false, false, false, false, false, false, false, false,     // 0x18 - 0x1F
            false, false, false, false, false, true,  false, false,     // 0x20 - 0x27
            false, false, false, false, false, true,  true,  false,     // 0x28 - 0x2F
            true,  true,  true,  true,  true,  true,  true,  true,      // 0x30 - 0x37
            true,  true,  true,  false, false, false, false, false,     // 0x38 - 0x3F
            false, true,  true,  true,  true,  true,  true,  true,      // 0x40 - 0x47
            true,  true,  true,  true,  true,  true,  true,  true,      // 0x48 - 0x4F
            true,  true,  true,  true,  true,  true,  true,  true,      // 0x50 - 0x57
            true,  true,  true,  true,  false, true,  false, false,     // 0x58 - 0x5F
            false, true,  true,  true,  true,  true,  true,  true,      // 0x60 - 0x67
            true,  true,  true,  true,  true,  true,  true,  true,      // 0x68 - 0x6F
            true,  true,  true,  true,  true,  true,  true,  true,      // 0x70 - 0x77
            true,  true,  true,  false, false, false, false, false,     // 0x78 - 0x7F
        };

        public static bool IsSafeHostStringChar(char c)
        {
            return c < SafeHostStringChars.Length && SafeHostStringChars[c];
        }
    }
}
