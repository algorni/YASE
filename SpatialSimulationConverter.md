# Spatial Simulation Converter

With the Spatial Simulation Converter tool you can convert a GeoJSON file with some specific type of geo feature (multiline) into YASE json format ready to be play back by the YASE Playback utility.

The overall idea is that every point of a multiline object reppresenting a location will be translated as an event. Every event will be played back with 5 second of distance between each other by default. 

This is extremely useful to test out complex event processors that must react to GPS coordinate changes (e.g. geo fancing checks or "on track" detection algorithms)

You can find the tool under */source/SpatialSimulationConverter*

## GeoJSON Format

Under */sample/enteringExitingTest.geojson* there is a sample of GEO Json file:

    {
    "type": "FeatureCollection",
    "name": "2021-7-2-enteringExitingTest",
    "crs": { "type": "name", "properties": { "name": "urn:ogc:def:crs:OGC:1.3:CRS84" } },
    "features": [
    { "type": "Feature", "properties": { "TrackerId": "EnteringStartArea" }, "geometry": { "type": "MultiLineString", "coordinates": [ [ [ 7.781101127752286, 44.256250557075134 ], [ 7.781340135153765, 44.255932644426828 ], [ 7.781459638854503, 44.255504682378358 ], [ 7.781314527217893, 44.255174538098593 ], [ 7.781143807645408, 44.254789367429588 ], [ 7.781177951559903, 44.254550927179721 ], [ 7.781263311346145, 44.254294144290945 ], [ 7.781305991239266, 44.254153524614836 ], [ 7.781399887004134, 44.254092385520337 ], [ 7.78151085472625, 44.254177980234829 ], [ 7.78146817483313, 44.254214663645776 ], [ 7.781408422982758, 44.254208549745535 ], [ 7.781382815046886, 44.254141296801009 ], [ 7.781186487538529, 44.254226891444347 ], [ 7.781263311346145, 44.254122955075545 ], [ 7.781254775367523, 44.254080157693821 ], [ 7.781365743089638, 44.254110727255359 ], [ 7.781408422982758, 44.254281916506393 ], [ 7.781527926683498, 44.254288030398996 ], [ 7.781527926683498, 44.254202435844682 ], [ 7.78146817483313, 44.254135182893165 ] ] ] } },
    { "type": "Feature", "properties": { "TrackerId": "ExitingArea" }, "geometry": { "type": "MultiLineString", "coordinates": [ [ [ 7.781425494940008, 44.254055702033149 ], [ 7.78139135102551, 44.254122955075545 ], [ 7.781374279068262, 44.254233005342677 ], [ 7.78151085472625, 44.254281916506393 ], [ 7.781544998640745, 44.254190208041017 ], [ 7.781382815046886, 44.254141296801009 ], [ 7.78128891928202, 44.253951765361769 ], [ 7.781365743089638, 44.253756119364418 ], [ 7.781451102875878, 44.253413737302829 ], [ 7.781638894405613, 44.253248659525177 ], [ 7.781826685935347, 44.253114151363754 ], [ 7.781741326149104, 44.253053011188683 ], [ 7.781348671132388, 44.253132493403889 ], [ 7.781160879602655, 44.253309799496868 ], [ 7.780879192308055, 44.253419851285685 ], [ 7.780785296543189, 44.253432079249528 ], [ 7.780606040992079, 44.253322027483577 ], [ 7.780785296543189, 44.253291457512027 ] ] ] } },
    { "type": "Feature", "properties": { "TrackerId": "movingOnSkilift" }, "geometry": { "type": "MultiLineString", "coordinates": [ [ [ 7.781323063196516, 44.254233005342677 ], [ 7.781271847324769, 44.254147410708256 ], [ 7.781399887004134, 44.254177980234829 ], [ 7.781476710811753, 44.254245233137411 ], [ 7.781493782769, 44.254208549745535 ], [ 7.781784006042225, 44.254025132442976 ], [ 7.781920581700215, 44.253860056381541 ], [ 7.782168125080317, 44.253737777518786 ], [ 7.782287628781057, 44.253664410079089 ], [ 7.78247542031079, 44.253499333005095 ], [ 7.782697355755019, 44.253395395350438 ], [ 7.782927827177874, 44.253211975511682 ], [ 7.78320097849385, 44.253040783146048 ], [ 7.78343998589533, 44.252942958713398 ], [ 7.78384117689067, 44.252673940684524 ], [ 7.784336263650877, 44.252423263686381 ], [ 7.784515519201986, 44.25228875363711 ], [ 7.784899638240076, 44.252013618487268 ], [ 7.785130109662931, 44.25189744992641 ], [ 7.785420332936157, 44.251671226280976 ], [ 7.786043459375728, 44.251310489478691 ], [ 7.786205642969589, 44.251145405797281 ], [ 7.786640977879424, 44.250913065016249 ], [ 7.786879985280905, 44.250760208738704 ], [ 7.787264104318997, 44.250552323563689 ], [ 7.787810406950947, 44.250154893978078 ], [ 7.788279885775283, 44.249916434937639 ], [ 7.788740828620992, 44.249580144904193 ], [ 7.78902251591559, 44.249311111493 ], [ 7.789406634953682, 44.249084877899307 ], [ 7.789713930184156, 44.248907559068918 ] ] ] } },
    { "type": "Feature", "properties": { "TrackerId": "arrivingLandingStation" }, "geometry": { "type": "MultiLineString", "coordinates": [ [ [ 7.789850505842144, 44.248797498836346 ], [ 7.790140729115368, 44.248638522581317 ], [ 7.790345592602348, 44.248467316903024 ], [ 7.790755319576315, 44.248253309104342 ], [ 7.790994326977794, 44.248069873228637 ], [ 7.791199190464774, 44.247953696878113 ], [ 7.79135283808001, 44.247886436780846 ], [ 7.791515021673873, 44.24777026006803 ], [ 7.791754029075351, 44.247599051862473 ], [ 7.791822316904343, 44.2475440205477 ], [ 7.791975964519582, 44.247574593506698 ], [ 7.792044252348574, 44.247709114337553 ], [ 7.79195035658371, 44.247764145497833 ], [ 7.791847924840217, 44.247727458063359 ], [ 7.791762565053975, 44.247782489206507 ] ] ] } }
    ]
    }

Git Hub has a nice render view for this:

![image](https://user-images.githubusercontent.com/45007019/131516910-0440e822-78d1-479d-9036-ba51589c24cc.png)

As you can see it's just a JSON file with a well defined structure. 
You can copy and paste and test out here if you want to play with this: https://geojson.io/

In GIS (Geographical Information Systems) you can define geometries of different types including Lines and Areas but also those MultiLineString 

I've choose this one since with open source tooling like QGIS generate those MultiLineString when creating interactively step by step lines clicking on the map on screen.    This is a super easy way to create your own path and try out your logic.

![image](https://user-images.githubusercontent.com/45007019/131516992-b74ca395-068e-4315-95a2-dbcaf2f4ace1.png)

Each feature requires an attribute named "TrackerId" that will be translated into the *Track Id* as well as the *SourceId* property in the YASEON format.

If you have multiple features (different multiline objects) those will be considered multiple "Tracks" in the YASEON according to their *TrackerId* attribute.

Here is an example of the YASE output of the tool:

    {
    "PlanTiming": 0,
    "SimulationLoops": null,
    "PlannedEventsTracks": {
        "EnteringStartArea": [
        {
            "EventOffset": "00:00:05",
            "SourceId": "EnteringStartArea",
            "Payload": {
            "Latitude": 44.256250557075134,
            "Longitude": 7.781101127752286
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "EnteringStartArea",
            "Payload": {
            "Latitude": 44.25593264442683,
            "Longitude": 7.781340135153765
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "EnteringStartArea",
            "Payload": {
            "Latitude": 44.25550468237836,
            "Longitude": 7.781459638854503
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "EnteringStartArea",
            "Payload": {
            "Latitude": 44.25517453809859,
            "Longitude": 7.781314527217893
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "EnteringStartArea",
            "Payload": {
            "Latitude": 44.25478936742959,
            "Longitude": 7.781143807645408
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "EnteringStartArea",
            "Payload": {
            "Latitude": 44.25455092717972,
            "Longitude": 7.781177951559903
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "EnteringStartArea",
            "Payload": {
            "Latitude": 44.254294144290945,
            "Longitude": 7.781263311346145
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "EnteringStartArea",
            "Payload": {
            "Latitude": 44.254153524614836,
            "Longitude": 7.781305991239266
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "EnteringStartArea",
            "Payload": {
            "Latitude": 44.25409238552034,
            "Longitude": 7.781399887004134
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "EnteringStartArea",
            "Payload": {
            "Latitude": 44.25417798023483,
            "Longitude": 7.78151085472625
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "EnteringStartArea",
            "Payload": {
            "Latitude": 44.254214663645776,
            "Longitude": 7.78146817483313
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "EnteringStartArea",
            "Payload": {
            "Latitude": 44.254208549745535,
            "Longitude": 7.781408422982758
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "EnteringStartArea",
            "Payload": {
            "Latitude": 44.25414129680101,
            "Longitude": 7.781382815046886
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "EnteringStartArea",
            "Payload": {
            "Latitude": 44.25422689144435,
            "Longitude": 7.781186487538529
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "EnteringStartArea",
            "Payload": {
            "Latitude": 44.254122955075545,
            "Longitude": 7.781263311346145
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "EnteringStartArea",
            "Payload": {
            "Latitude": 44.25408015769382,
            "Longitude": 7.781254775367523
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "EnteringStartArea",
            "Payload": {
            "Latitude": 44.25411072725536,
            "Longitude": 7.781365743089638
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "EnteringStartArea",
            "Payload": {
            "Latitude": 44.25428191650639,
            "Longitude": 7.781408422982758
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "EnteringStartArea",
            "Payload": {
            "Latitude": 44.254288030398996,
            "Longitude": 7.781527926683498
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "EnteringStartArea",
            "Payload": {
            "Latitude": 44.25420243584468,
            "Longitude": 7.781527926683498
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "EnteringStartArea",
            "Payload": {
            "Latitude": 44.254135182893165,
            "Longitude": 7.78146817483313
            }
        }
        ],
        "ExitingArea": [
        {
            "EventOffset": "00:00:05",
            "SourceId": "ExitingArea",
            "Payload": {
            "Latitude": 44.25405570203315,
            "Longitude": 7.781425494940008
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "ExitingArea",
            "Payload": {
            "Latitude": 44.254122955075545,
            "Longitude": 7.78139135102551
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "ExitingArea",
            "Payload": {
            "Latitude": 44.25423300534268,
            "Longitude": 7.781374279068262
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "ExitingArea",
            "Payload": {
            "Latitude": 44.25428191650639,
            "Longitude": 7.78151085472625
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "ExitingArea",
            "Payload": {
            "Latitude": 44.25419020804102,
            "Longitude": 7.781544998640745
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "ExitingArea",
            "Payload": {
            "Latitude": 44.25414129680101,
            "Longitude": 7.781382815046886
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "ExitingArea",
            "Payload": {
            "Latitude": 44.25395176536177,
            "Longitude": 7.78128891928202
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "ExitingArea",
            "Payload": {
            "Latitude": 44.25375611936442,
            "Longitude": 7.781365743089638
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "ExitingArea",
            "Payload": {
            "Latitude": 44.25341373730283,
            "Longitude": 7.781451102875878
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "ExitingArea",
            "Payload": {
            "Latitude": 44.25324865952518,
            "Longitude": 7.781638894405613
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "ExitingArea",
            "Payload": {
            "Latitude": 44.253114151363754,
            "Longitude": 7.781826685935347
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "ExitingArea",
            "Payload": {
            "Latitude": 44.25305301118868,
            "Longitude": 7.781741326149104
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "ExitingArea",
            "Payload": {
            "Latitude": 44.25313249340389,
            "Longitude": 7.781348671132388
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "ExitingArea",
            "Payload": {
            "Latitude": 44.25330979949687,
            "Longitude": 7.781160879602655
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "ExitingArea",
            "Payload": {
            "Latitude": 44.253419851285685,
            "Longitude": 7.780879192308055
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "ExitingArea",
            "Payload": {
            "Latitude": 44.25343207924953,
            "Longitude": 7.780785296543189
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "ExitingArea",
            "Payload": {
            "Latitude": 44.25332202748358,
            "Longitude": 7.780606040992079
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "ExitingArea",
            "Payload": {
            "Latitude": 44.25329145751203,
            "Longitude": 7.780785296543189
            }
        }
        ],
        "movingOnSkilift": [
        {
            "EventOffset": "00:00:05",
            "SourceId": "movingOnSkilift",
            "Payload": {
            "Latitude": 44.25423300534268,
            "Longitude": 7.781323063196516
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "movingOnSkilift",
            "Payload": {
            "Latitude": 44.254147410708256,
            "Longitude": 7.781271847324769
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "movingOnSkilift",
            "Payload": {
            "Latitude": 44.25417798023483,
            "Longitude": 7.781399887004134
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "movingOnSkilift",
            "Payload": {
            "Latitude": 44.25424523313741,
            "Longitude": 7.781476710811753
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "movingOnSkilift",
            "Payload": {
            "Latitude": 44.254208549745535,
            "Longitude": 7.781493782769
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "movingOnSkilift",
            "Payload": {
            "Latitude": 44.254025132442976,
            "Longitude": 7.781784006042225
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "movingOnSkilift",
            "Payload": {
            "Latitude": 44.25386005638154,
            "Longitude": 7.781920581700215
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "movingOnSkilift",
            "Payload": {
            "Latitude": 44.253737777518786,
            "Longitude": 7.782168125080317
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "movingOnSkilift",
            "Payload": {
            "Latitude": 44.25366441007909,
            "Longitude": 7.782287628781057
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "movingOnSkilift",
            "Payload": {
            "Latitude": 44.253499333005095,
            "Longitude": 7.78247542031079
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "movingOnSkilift",
            "Payload": {
            "Latitude": 44.25339539535044,
            "Longitude": 7.782697355755019
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "movingOnSkilift",
            "Payload": {
            "Latitude": 44.25321197551168,
            "Longitude": 7.782927827177874
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "movingOnSkilift",
            "Payload": {
            "Latitude": 44.25304078314605,
            "Longitude": 7.78320097849385
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "movingOnSkilift",
            "Payload": {
            "Latitude": 44.2529429587134,
            "Longitude": 7.78343998589533
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "movingOnSkilift",
            "Payload": {
            "Latitude": 44.252673940684524,
            "Longitude": 7.78384117689067
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "movingOnSkilift",
            "Payload": {
            "Latitude": 44.25242326368638,
            "Longitude": 7.784336263650877
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "movingOnSkilift",
            "Payload": {
            "Latitude": 44.25228875363711,
            "Longitude": 7.784515519201986
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "movingOnSkilift",
            "Payload": {
            "Latitude": 44.25201361848727,
            "Longitude": 7.784899638240076
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "movingOnSkilift",
            "Payload": {
            "Latitude": 44.25189744992641,
            "Longitude": 7.785130109662931
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "movingOnSkilift",
            "Payload": {
            "Latitude": 44.251671226280976,
            "Longitude": 7.785420332936157
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "movingOnSkilift",
            "Payload": {
            "Latitude": 44.25131048947869,
            "Longitude": 7.786043459375728
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "movingOnSkilift",
            "Payload": {
            "Latitude": 44.25114540579728,
            "Longitude": 7.786205642969589
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "movingOnSkilift",
            "Payload": {
            "Latitude": 44.25091306501625,
            "Longitude": 7.786640977879424
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "movingOnSkilift",
            "Payload": {
            "Latitude": 44.250760208738704,
            "Longitude": 7.786879985280905
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "movingOnSkilift",
            "Payload": {
            "Latitude": 44.25055232356369,
            "Longitude": 7.787264104318997
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "movingOnSkilift",
            "Payload": {
            "Latitude": 44.25015489397808,
            "Longitude": 7.787810406950947
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "movingOnSkilift",
            "Payload": {
            "Latitude": 44.24991643493764,
            "Longitude": 7.788279885775283
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "movingOnSkilift",
            "Payload": {
            "Latitude": 44.24958014490419,
            "Longitude": 7.788740828620992
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "movingOnSkilift",
            "Payload": {
            "Latitude": 44.249311111493,
            "Longitude": 7.78902251591559
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "movingOnSkilift",
            "Payload": {
            "Latitude": 44.24908487789931,
            "Longitude": 7.789406634953682
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "movingOnSkilift",
            "Payload": {
            "Latitude": 44.24890755906892,
            "Longitude": 7.789713930184156
            }
        }
        ],
        "arrivingLandingStation": [
        {
            "EventOffset": "00:00:05",
            "SourceId": "arrivingLandingStation",
            "Payload": {
            "Latitude": 44.248797498836346,
            "Longitude": 7.789850505842144
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "arrivingLandingStation",
            "Payload": {
            "Latitude": 44.24863852258132,
            "Longitude": 7.790140729115368
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "arrivingLandingStation",
            "Payload": {
            "Latitude": 44.248467316903024,
            "Longitude": 7.790345592602348
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "arrivingLandingStation",
            "Payload": {
            "Latitude": 44.24825330910434,
            "Longitude": 7.790755319576315
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "arrivingLandingStation",
            "Payload": {
            "Latitude": 44.24806987322864,
            "Longitude": 7.790994326977794
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "arrivingLandingStation",
            "Payload": {
            "Latitude": 44.24795369687811,
            "Longitude": 7.791199190464774
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "arrivingLandingStation",
            "Payload": {
            "Latitude": 44.247886436780846,
            "Longitude": 7.79135283808001
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "arrivingLandingStation",
            "Payload": {
            "Latitude": 44.24777026006803,
            "Longitude": 7.791515021673873
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "arrivingLandingStation",
            "Payload": {
            "Latitude": 44.24759905186247,
            "Longitude": 7.791754029075351
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "arrivingLandingStation",
            "Payload": {
            "Latitude": 44.2475440205477,
            "Longitude": 7.791822316904343
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "arrivingLandingStation",
            "Payload": {
            "Latitude": 44.2475745935067,
            "Longitude": 7.791975964519582
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "arrivingLandingStation",
            "Payload": {
            "Latitude": 44.24770911433755,
            "Longitude": 7.792044252348574
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "arrivingLandingStation",
            "Payload": {
            "Latitude": 44.24776414549783,
            "Longitude": 7.79195035658371
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "arrivingLandingStation",
            "Payload": {
            "Latitude": 44.24772745806336,
            "Longitude": 7.791847924840217
            }
        },
        {
            "EventOffset": "00:00:05",
            "SourceId": "arrivingLandingStation",
            "Payload": {
            "Latitude": 44.24778248920651,
            "Longitude": 7.791762565053975
            }
        }
        ]
    }
    }

## The Tool

The tool is just a .NET Core Console application and requires a couple of parameters which can be provided by an app settrings file (appsettings.json) or from command line.

| Parameter | Description |
| --- | --- |
| geoJsonFile | Full file path of the GEO Json input file |
| simulationPlanFile | Output file path  in YEASON format | 

