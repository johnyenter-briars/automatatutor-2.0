set SCRIPT_DIR=%~dp0
java -XX:+CMSClassUnloadingEnabled -Xmx1024M -Xss2M -jar "%SCRIPT_DIR%\sbt-launch.jar" %*
