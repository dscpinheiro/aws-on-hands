'use strict';

exports.handler = (event, context, callback) => {
    const response = event.Records[0].cf.response;
    const headers = response.headers;

    headers['strict-transport-security'] = [{
        key: 'Strict-Transport-Security', 
        value: 'max-age=63072000; includeSubdomains; preload'
    }];
 
    headers['x-content-type-options'] = [{
        key: 'X-Content-Type-Options', 
        value: 'nosniff'
    }]; 

    headers['x-frame-options'] = [{
        key: 'X-Frame-Options', 
        value: 'DENY'
    }]; 

    headers['x-xss-protection'] = [{
        key: 'X-XSS-Protection', 
        value: '1; mode=block'
    }]; 

    headers['referrer-policy'] = [{
        key: 'Referrer-Policy', 
        value: 'same-origin'
    }];

    headers['content-security-policy'] = [{
        key: 'Content-Security-Policy', 
        value: "default-src 'none'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self'; font-src 'self' https://cdn.joinhoney.com/fonts/icons/; object-src 'self'; frame-ancestors 'none'; form-action 'none'; base-uri 'self'; report-uri https://my.report-uri.com/r/d/csp/enforce;"
    }];

    callback(null, response);
};