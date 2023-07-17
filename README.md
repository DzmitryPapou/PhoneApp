# Phone App
Test task for Texode company.

Develop a client-server application to work with arbitrary
information cards. Information card consists of 2 elements
information: the name of the card and the graphic image. as options
information, you can use the following types: goods in the store, books, mobile
telephones, etc.

Primary requirements
The server application must work over the HTTP REST protocol and use
.NET Core 2.0+. The server application is a Windows service or a regular
A Windows application that provides an HTTP API to a client application. Client
side must be a Windows application using technology
building GUIs WPF .NET Framework 4.7

Server part
- loading information cards from a file on the server side. File format on
choice - JSON, XML
- saving added and modified information cards to a file on the side
servers
- processing user GUI requests for CRUD operations with information
cards
- error handling while working with the client

Client side
- displaying a list of information cards in the GUI of the client application with
display of graphic images for each information card
- using the MVVM pattern
- support for CRUD operations for information cards
- error handling when working with the server
- graphic images can be in JPG, PNG format. Enough Support
one of the formats
- sorting information cards by name (optional)
- the ability to delete several information cards in one operation (not
mandatory function)
- checking for errors in entering information cards - an empty name and
missing image (optional)
