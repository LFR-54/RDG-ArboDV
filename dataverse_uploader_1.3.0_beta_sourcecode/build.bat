@echo off
set "JAVA_HOME=C:\Program Files\Eclipse Adoptium\jdk-17.0.19.10-hotspot"
set "PATH=C:\Program Files\Eclipse Adoptium\jdk-17.0.19.10-hotspot\bin;%PATH%"
echo JAVA_HOME is set to: %JAVA_HOME%
echo Running maven package...
call "D:\maven\apache-maven-3.9.16\bin\mvn.cmd" clean package assembly:single
