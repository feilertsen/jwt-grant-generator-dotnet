# Maskinporten client authentication example

An example on how to retrieve tokens for accessing Maskinporten protected APIs (fdir, ID-porten, etc.). See https://github.com/difi/jwt-grant-generator for more thorough information.

> Scopes, claims and grant type might differ depending on your usage (service provider).


The technique is described [here](https://openid.net/specs/openid-connect-core-1_0.html#ClientAuthentication) and is based on the OAuth JWT assertion specification ([RFC 7523](https://tools.ietf.org/html/rfc7523)).


> **NB:** Not production ready code. I take no liability for usage.
