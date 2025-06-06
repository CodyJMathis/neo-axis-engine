#if !NO_UI_WEB_BROWSER
namespace Internal.Xilium.CefGlue
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using Internal.Xilium.CefGlue.Interop;

    /// <summary>
    /// Class representing the issuer or subject field of an X.509 certificate.
    /// </summary>
    public sealed unsafe partial class CefSslCertPrincipal
    {
        /// <summary>
        /// Returns a name that can be used to represent the issuer.  It tries in this
        /// order: CN, O and OU and returns the first non-empty one found.
        /// </summary>
        public string DisplayName
        {
            get
            {
                var n_result = cef_sslcert_principal_t.get_display_name(_self);
                return cef_string_userfree.ToString(n_result);
            }
        }

        /// <summary>
        /// Returns the common name.
        /// </summary>
        public string CommonName
        {
            get
            {
                var n_result = cef_sslcert_principal_t.get_common_name(_self);
                return cef_string_userfree.ToString(n_result);
            }
        }

        /// <summary>
        /// Returns the locality name.
        /// </summary>
        public string LocalityName
        {
            get
            {
                var n_result = cef_sslcert_principal_t.get_locality_name(_self);
                return cef_string_userfree.ToString(n_result);
            }
        }

        /// <summary>
        /// Returns the state or province name.
        /// </summary>
        public string StateOrProvinceName
        {
            get
            {
                var n_result = cef_sslcert_principal_t.get_state_or_province_name(_self);
                return cef_string_userfree.ToString(n_result);
            }
        }

        /// <summary>
        /// Returns the country name.
        /// </summary>
        public string CountryName
        {
            get
            {
                var n_result = cef_sslcert_principal_t.get_country_name(_self);
                return cef_string_userfree.ToString(n_result);
            }
        }

        /// <summary>
        /// Retrieve the list of street addresses.
        /// </summary>
        public string[] StreetAddresses
        {
            get
            {
                cef_string_list* n_result = libcef.string_list_alloc();
                cef_sslcert_principal_t.get_street_addresses(_self, n_result);
                var result = cef_string_list.ToArray(n_result);
                libcef.string_list_free(n_result);
                return result;
            }
        }

        /// <summary>
        /// Retrieve the list of organization names.
        /// </summary>
        public string[] OrganizationNames
        {
            get
            {
                cef_string_list* n_result = libcef.string_list_alloc();
                cef_sslcert_principal_t.get_organization_names(_self, n_result);
                var result = cef_string_list.ToArray(n_result);
                libcef.string_list_free(n_result);
                return result;
            }
        }

        /// <summary>
        /// Retrieve the list of organization unit names.
        /// </summary>
        public string[] OrganizationUnitNames
        {
            get
            {
                cef_string_list* n_result = libcef.string_list_alloc();
                cef_sslcert_principal_t.get_organization_unit_names(_self, n_result);
                var result = cef_string_list.ToArray(n_result);
                libcef.string_list_free(n_result);
                return result;
            }
        }

        /// <summary>
        /// Retrieve the list of domain components.
        /// </summary>
        public string[] DomainComponents
        {
            get
            {
                cef_string_list* n_result = libcef.string_list_alloc();
                cef_sslcert_principal_t.get_domain_components(_self, n_result);
                var result = cef_string_list.ToArray(n_result);
                libcef.string_list_free(n_result);
                return result;
            }
        }
    }
}

#endif