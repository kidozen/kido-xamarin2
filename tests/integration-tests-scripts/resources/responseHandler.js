var colors = require('colors');

var stringify = require('stringify');

exports.processResponse = function(err, res, body,testName){

		console.log('\n'+testName);

        if(err){
            console.log(colors.red(err));
        	}
    	
    	else if(res.statusCode == 200 || res.statusCode == 201){
    		console.log(colors.green(res.statusCode));
			console.log(colors.green(JSON.stringify(body, null, 4)));
			}

		else{
			console.log(colors.red(res.statusCode));
			console.log(colors.red(JSON.stringify(body, null, 4)));
			}

}
