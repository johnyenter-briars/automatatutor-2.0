# this dockerfile is for local testing only

# run Docker build in two steps to speed up development
# 1) load sbt environment with sbt-environment.Dockerfile
# 2) compile and run application
FROM sbt-environment

# run the local application in docker, i.e. mount local directory at /app/
ADD . /app/
WORKDIR /app

# use special property settings for docker
COPY src/main/resources/props/default.props.docker.template /app/src/main/resources/props/default.props

###
# run applicaiton in normal mode
ENTRYPOINT ["sbt", "~container:start"]

###
# run application in debug mode
#CMD java -agentlib:jdwp=transport=dt_socket,address=9009,server=y,suspend=y -jar sbt-launch.jar ~container:start
