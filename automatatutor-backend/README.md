# How to Run Automata-tutor Backend on Linux

#### Prerequisites

To run the AT backend on linux, you need mono and msbuild installed. 

Instructions can be installed at: https://www.mono-project.com/download/stable/

REQUIRED Packages:
- mono-devel
- mono-complete
- mono-dbg
- referenceassemblies-pcl
- ca-certificates-mono
- mono-xsp4

##### Build the Project

Next, move to the root of the backend project at automata-tutor-master/ and run:
```
msbuild
```

You will see some errors akin to "The type or namespace name 'VisualStudio' does not exist in the namespace 'Microsoft' (are you missing an assembly reference?)"

This is because the tests were build using the visual studio suite, which obviously do not exist in linux.

#### Run XSP

Next, move within the WebServicePDL/ project and run:

```
xsp4 --port=53861
```

Note: the port must be set at 53981 becuase that port is what's expected by the front end

To check that the back end is working, navigate in a web browser to http://localhost:53861/Service1.asmx and you should see the SOAP interface.