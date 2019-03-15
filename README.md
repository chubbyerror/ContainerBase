# ContainerBase
## Info:
This is a Dubbo-like Distributed Microservice Framework on .Net Standard(>2.0),but more primitive.

### layer

As Dubbo, we had these layers(not all):

##### Registry:
Server Discovery. 

##### Protocol:
As title.

##### Transport:
As title.

##### Serialize:
Temporary use newtownsoft and string.

I've already made client-packages and sever packages,also made them easy to use.But a distributed microservice framework can't be easy.

### Other
There's some useful tools in KdZookeeper.Standard and Kdetcd.Standard ,enjoy it!

it's a personal project, so it should not be used in production .

a little benchmark here:

mvc 100 retuns use 526ms .
grpc  100 retuns use 359ms .
thrift  100 retuns use 199ms .
g+t mix 100 retuns use 316ms .
