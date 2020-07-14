# Updates made to Automata-Tutor

This document outlines the additions, deletions, and refactoring that were implimented on AT web application from 06/17/2020 - ____

## Folders

The idea is that the notion of "loose" questions is not practical, and leads to more overhead for admins/instructors. 

Therefore, a "folder" system is implimented to allow for the grouping of related problems. In this, folders are now posed instead of individual problems being posed. 

You can now delete, edit, and pose folders. You can add an unlimited number of problems to each folder. When you delete a folder, its containing problems go back to a "courseless" state, and will be shown in the autogen/index page. When you delete a course, all of the folders are deleted but the problems are preserved as previously specified. 

#### Implementation of Folderes

This was done by following the Lift patterns laid out previously, and "lifting" (hehe) the appropiate code logic from the Problem class into a new scala class called Folder. 

In addition, the database also has records to connect problems to folders, in an identical manor as users are connected to courses. 

NOTE: Even though the ability to pose a problem was removed, the code for posing a problem still exists with the problem class, and elsewhere within the code base. It is just removed from any front end connections.


## Contributing
Pull requests are welcome. Do with it what you will. Idk man, I barely have a degree. 

## License
See LICENSE