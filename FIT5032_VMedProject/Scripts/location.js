// References
// 1. Local search with the Geocoding API -https://docs.mapbox.com/help/tutorials/local-search-geocoding-api/
// 2. Locate the user (Geolocation) -https://docs.mapbox.com/mapbox-gl-js/example/locate-user/
// 3. Display zoom and rotation controls -https://docs.mapbox.com/mapbox-gl-js/example/navigation/
// 4. Measure distances -https://docs.mapbox.com/mapbox-gl-js/example/measure/
// Access Token Variable
const TOKEN = "";
var locations = [];

// Obtain all the latitude and longitude from the HTML
$(".coordinates").each(function () {
    var longitude = $(".longitude", this).text().trim();
    var latitude = $(".latitude", this).text().trim();
    var description = $(".description", this).text().trim();
    var clinicname = $(".clinicname", this).text().trim();
    // Create a point data structure to hold the values.
    var point = {
        "latitude": latitude,
        "longitude": longitude,
        "description": description,
        "clinicname": clinicname
    };
    // Push them all into an array.
    locations.push(point);
});
// Initialize the map
mapboxgl.accessToken = TOKEN;
var map = new mapboxgl.Map({
    container: 'map',
    style: 'mapbox://styles/mapbox/streets-v12',
    zoom: 11,
    center: [144.9631, -37.8136] // Center the map on Melbourne [lng, lat]
});
const geocoder = new MapboxGeocoder({
    // Initialize the geocoder
    accessToken: mapboxgl.accessToken, // Set the access token
    mapboxgl: mapboxgl, // Set the mapbox-gl instance
    marker: false, // Do not use the default marker style
    placeholder: 'Search for places in Melbourne', // Placeholder text for the search bar
    bbox: [144.5619, -38.4339, 145.5125, -37.5917], // Boundary for Melbourne
    proximity: {
        longitude: 144.9631,
        latitude: -37.8136
    } // Coordinates of Melbourne
});
// Add the geocoder to the map
map.addControl(geocoder);

// After the map style has loaded on the page, add a source layer for your locations
map.on('load', () => {
    map.addSource('locations', {
        type: 'geojson',
        data: {
            type: 'FeatureCollection',
            features: locations.map(location => {
                // Create a marker for each location
                const marker = new mapboxgl.Marker({
                    color: "#FF0000"
                }).setLngLat([parseFloat(location.longitude), parseFloat(location.latitude)]).addTo(map);

                // Create a popup for each location with formatted content
                const popupContent = `
                <strong>Clinic Name:</strong> <br> ${location.clinicname}<br>
                <strong>Address:</strong> <br> ${location.description}<br>
                <strong>Latitude:</strong> <br> ${location.latitude}<br>
                <strong>Longitude:</strong> <br> ${location.longitude}`;

                const popup = new mapboxgl.Popup()
                    .setHTML(popupContent);

                // Attach the popup to the marker
                marker.setPopup(popup);

                return {
                    type: 'Feature',
                    geometry: {
                        type: 'Point',
                        coordinates: [parseFloat(location.longitude), parseFloat(location.latitude)]
                    },
                    properties: {
                        description: location.description
                    }
                };
            })
        }
    });

    map.addLayer({
        id: 'locations',
        source: 'locations',
        type: 'circle',
        paint: {
            'circle-radius': 5,
            'circle-color': '#448ee4'
        }
    });

    // Listen for the `result` event from the Geocoder
    // `result` event is triggered when a user makes a selection
    geocoder.on('result', (event) => {
        map.getSource('single-point').setData(event.result.geometry);
    });
});

// Add zoom and rotation controls to the map.
map.addControl(new mapboxgl.NavigationControl());

// Add geolocate control to the map.
map.addControl(
    new mapboxgl.GeolocateControl({
        positionOptions: {
            enableHighAccuracy: true
        },
        // When active the map will receive updates to the device's location as it changes.
        trackUserLocation: true,
        // Draw an arrow next to the location dot to indicate which direction the device is heading.
        showUserHeading: true
    })
);

const distanceContainer = document.getElementById('distance');

// GeoJSON object to hold our measurement features
const geojson = {
    'type': 'FeatureCollection',
    'features': []
};

// Used to draw a line between points
const linestring = {
    'type': 'Feature',
    'geometry': {
        'type': 'LineString',
        'coordinates': []
    }
};

map.on('load', () => {
    map.addSource('geojson', {
        'type': 'geojson',
        'data': geojson
    });

    // Add styles to the map
    map.addLayer({
        id: 'measure-points',
        type: 'circle',
        source: 'geojson',
        paint: {
            'circle-radius': 5,
            'circle-color': '#000'
        },
        filter: ['in', '$type', 'Point']
    });
    map.addLayer({
        id: 'measure-lines',
        type: 'line',
        source: 'geojson',
        layout: {
            'line-cap': 'round',
            'line-join': 'round'
        },
        paint: {
            'line-color': '#000',
            'line-width': 2.5
        },
        filter: ['in', '$type', 'LineString']
    });

    map.on('click', (e) => {
        const features = map.queryRenderedFeatures(e.point, {
            layers: ['measure-points']
        });

        // Remove the linestring from the group
        // so we can redraw it based on the points collection.
        if (geojson.features.length > 1) geojson.features.pop();

        // Clear the distance container to populate it with a new value.
        distanceContainer.innerHTML = '';

        // If a feature was clicked, remove it from the map.
        if (features.length) {
            const id = features[0].properties.id;
            geojson.features = geojson.features.filter(
                (point) => point.properties.id !== id
            );
        } else {
            const point = {
                'type': 'Feature',
                'geometry': {
                    'type': 'Point',
                    'coordinates': [e.lngLat.lng, e.lngLat.lat]
                },
                'properties': {
                    'id': String(new Date().getTime())
                }
            };

            geojson.features.push(point);
        }

        if (geojson.features.length > 1) {
            linestring.geometry.coordinates = geojson.features.map(
                (point) => point.geometry.coordinates
            );

            geojson.features.push(linestring);

            // Populate the distanceContainer with total distance
            const value = document.createElement('pre');
            const distance = turf.length(linestring);
            value.textContent = `Total distance: ${distance.toLocaleString()}km`;
            distanceContainer.appendChild(value);
        }

        map.getSource('geojson').setData(geojson);
    });
});

map.on('mousemove', (e) => {
    const features = map.queryRenderedFeatures(e.point, {
        layers: ['measure-points']
    });
    // Change the cursor to a pointer when hovering over a point on the map.
    // Otherwise cursor is a crosshair.
    map.getCanvas().style.cursor = features.length
        ? 'pointer'
        : 'crosshair';
});
