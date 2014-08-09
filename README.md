SharpConfig
===========

Easy configuration

Why?
-----------

To me, the nowadays ubiquitous 'config transforms' used in dotnet are overly complex, adding alot of unecessary cognitive load. The main issue is that
it is generally impossible to easily envisage what the transformed file will look like by simply examining the inputs. Typically whole swathes
of input config file are completely replaced with text that may not even resemble what it is replacing. This is seldom needed for normal configuration.

Additionally the standard methods use xslt, and operate on xml files only; on the other hand other auxiliary file types may also benefit from
transformation. However the xslt method does not support these. For example
-  ini files  
-  batch files  
-  simple scripts  

The main benefit I see from this program is the increased simplicity in small-scale configuration management. It is clear what the final configuration files
will look like, and to determine the actual values for a specific environment is a simple matter of looking them up from a tabular source (csv file). The same values
can also be easily shared across multiple projects in a solution, and multiple file-types, something not easy to achieve using the xslt transform approach.

What?
-----------

SharpConfig.exe is a simple, self-contained exe that takes as input files with placeholders like ${username} in them, and produces output files,
one for each 'environment'. The environments, plus the values that should be substituted for the placeholders are loaded from a specified csv file. 
In theory this could be extended to read from diverse 'configuration sources' such as databases or http services.

For each input template file containing placeholders, the program outputs a file for each environment, with each output file identical to the input file,
save that the placeholders will have been replaced by the appropriate values specific to that environment. 

The program can then optionally copy one output file to another file. For example:
take web.template.config as the input template file (NOT web.config, as will be explained below).
running SharpConfig.exe might produce, for example, the following files:
web.template.config=>  
web.dev.config  
web.uat.config  
web.prod.config  

By default, SharpConfig will then copy  
web.dev.config=>web.config  

Therefore in development, your web.config file will be a proper config file (no placeholders), with the correct values in place for the dev environment.
This copying is controlled by the --default-enviornment option (by default it is set to "dev"). To disable copying, set it to "".


When used within Visual Studio, an invocation to SharpConfig can be put in as a pre-build event. I have found this to be very effective. However, there is nothing specific in 
the program tying it to dot net development. It can be used stand-alone or integrated in any Windows-based development.

More detail
-----------

This configuration works off 'template' files, which are the same shape as the final files you want, but which have placeholders
for values that can change between environments. Examples of such values might be email servers, usernames or passwords. Placeholders
are expressed by the ${xxx} notation. 

The outside of a placeholder is the leading ${ and the trailing }. You can escape the leading ${ by using ${{ if your file needs a literal '${' in it.

The content of a placeholder is everything between the leading ${ and the trailing }. This is known as a configuration key. Configuration keys
can have any characters except a }. For clarity, it would probably be better to use normal alpha-numeric characters plus some punctuation such as
. and -.

The values for configuration keys are kept in a csv file. By default this file is called ConfigValues.csv, but you can use any file.
Also by default the csv file is comma delimited, and double-quotes (") are used to mark fields containing commas and/or linebreaks. These values 
are also configurable.

The contents of the csv file are arranged like this. The first column contains all the configuration keys, and the first row contains all the environments you will need values for.
The rest of the cells contain values to be used for specific keys and environments. The top-left value is not used. For example:

,dev,uat,prod  
db.server,localhost,uat-server,prod-server  
db.username,dev_user,uat_user,prod_user  
db.password,dev_pw,uat_pw,prod_pw  


describes 3 environments (dev, uat, prod) and 3 configuration keys (db.server, db.username, db.password). The specific values for these
keys in any given environment are found by cross-referencing the key with the envioronment. For example, the value for 'db.username' in 'uat'
is uat_user

In a template file like web.template.config, we might have for example: 
Server=${db.server}, Username=${db.username}, Password=${db.password}

Running SharpConfig.exe would produce the following output files, with the following values:
web.dev.config: 
Server=localhost, Username=dev_user, Password=dev_password

web.uat.config: 
Server=uat-server, Username=uat_user, Password=uat_password

web.prod.config: 
Server=prod-server, Username=prod_user, Password=prod_password


By default, the program will additionally copy the contents of web.dev.config to web.config, meaning as a developer everything will function normally. 
This is controlled by the --default-enviornment option.

