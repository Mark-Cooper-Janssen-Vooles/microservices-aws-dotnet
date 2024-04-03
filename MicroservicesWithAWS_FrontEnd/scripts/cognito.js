const config={
    cognito:{
        identityPoolId:"ap-southeast-2_Qbyfndbgt",
        cognitoDomain:"hotel-booking-service-app.auth.ap-southeast-2.amazoncognito.com",
        appId:"612ddhu2iuoc1k96auknmiufad"
    }
}

var cognitoApp={
    auth:{},
    Init: function()
    {

        var authData = {
            ClientId : config.cognito.appId,
            AppWebDomain : config.cognito.cognitoDomain,
            TokenScopesArray : ['email', 'openid'],
            RedirectUriSignIn : 'http://localhost:6060/hotel/',
            RedirectUriSignOut : 'http://localhost:6060/hotel/',
            UserPoolId : config.cognito.identityPoolId, 
            AdvancedSecurityDataCollectionFlag : false,
                Storage: null
        };

        cognitoApp.auth = new AmazonCognitoIdentity.CognitoAuth(authData);
        cognitoApp.auth.userhandler = {
            onSuccess: function(result) {
              
            },
            onFailure: function(err) {
            }
        };
    }
}