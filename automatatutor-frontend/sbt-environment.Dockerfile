# This dockerfile is for local testing only.
# It loads the sbt and java environment.

FROM 1science/sbt:0.13.8-oracle-jre-8

# run the local application in docker, i.e. mount local directory at /app/
ADD . /app/
WORKDIR /app

# just load all sbt libraries and quit
RUN sbt clean
