By default, SharpConfig is configured to use the configuration values found in ConfigValues.csv in the same directory as this readme file.
In addition it is configured to use any overrides found in the file SharpConfig.extends.xml

Either of these files may also be moved to the solution directory in order to be shared among multiple projects.

However, any values found in the copy of SharpConfig.extends.xml found in *this* directory will override any others.