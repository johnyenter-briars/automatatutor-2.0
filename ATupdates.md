# Updates made to Automata-Tutor
This document outlines the additions, deletions, and refactoring that were implemented on AT web application from 06/17/2020 - ____

## Folders
The idea is that the notion of "loose" questions is not practical, and leads to more overhead for admins/instructors. 

Therefore, a "folder" system is implemented to allow for the grouping of related problems. In this, folders are now posed instead of individual problems being posed. 

You can now delete, edit, and pose folders. You can add an unlimited number of problems to each folder. When you delete a folder, its containing problems go back to a "courseless" state, and will be shown in the autogen/index page. When you delete a course, all of the folders are deleted but the problems are preserved as previously specified. 

## Shared Problems
Originally problems were discrete units and were "sent" to courses. AT frontend was updated so that courses and folders held links to problems - instead of problems themselves. (Referred to as ProblemPointer) This enables problems to be "shared" across multiple courses or potentially folders. 


## Contributing
Pull requests are welcome. Do with it what you will. Idk man, I barely have a degree. 

## License
See LICENSE