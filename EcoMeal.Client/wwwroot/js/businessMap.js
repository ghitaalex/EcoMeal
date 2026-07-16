const maps = new Map();

export function getCurrentLocation() {
    return new Promise((resolve) => {
        if (!navigator.geolocation) {
            resolve({ success: false, error: "Location is not supported by this browser." });
            return;
        }

        navigator.geolocation.getCurrentPosition(
            (position) => resolve({
                success: true,
                latitude: position.coords.latitude,
                longitude: position.coords.longitude,
                accuracyMeters: position.coords.accuracy
            }),
            (error) => {
                const messages = {
                    1: "Location permission was denied. You can enable it in your browser settings.",
                    2: "Your location is currently unavailable.",
                    3: "Finding your location took too long. Please try again."
                };
                resolve({ success: false, error: messages[error.code] || "Unable to find your location." });
            },
            { enableHighAccuracy: false, timeout: 10000, maximumAge: 300000 }
        );
    });
}

export function renderBusinessMap(elementId, businesses, userLocation) {
    if (!window.L)
        throw new Error("The map library could not be loaded.");

    disposeBusinessMap(elementId);

    const element = document.getElementById(elementId);
    if (!element)
        return;

    const map = L.map(element, { scrollWheelZoom: true, zoomControl: true });
    maps.set(elementId, map);

    L.tileLayer("https://tile.openstreetmap.org/{z}/{x}/{y}.png", {
        maxZoom: 19,
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
    }).addTo(map);

    const bounds = [];

    for (const business of businesses || []) {
        if (!Number.isFinite(business.latitude) || !Number.isFinite(business.longitude))
            continue;

        const coordinates = [business.latitude, business.longitude];
        bounds.push(coordinates);

        const icon = L.divIcon({
            className: "business-map-marker",
            html: '<span aria-hidden="true">●</span>',
            iconSize: [34, 34],
            iconAnchor: [17, 17],
            popupAnchor: [0, -18]
        });
        const marker = L.marker(coordinates, {
            icon,
            title: business.name,
            alt: `${business.name} map marker`,
            riseOnHover: true
        }).addTo(map);

        marker.bindPopup(createBusinessPopup(business), { minWidth: 210 });
    }

    if (userLocation && Number.isFinite(userLocation.latitude) && Number.isFinite(userLocation.longitude)) {
        const userCoordinates = [userLocation.latitude, userLocation.longitude];
        bounds.push(userCoordinates);
        L.circleMarker(userCoordinates, {
            radius: 9,
            color: "#ffffff",
            weight: 3,
            fillColor: "#23875a",
            fillOpacity: 1
        }).addTo(map).bindTooltip("Your location");

        if (Number.isFinite(userLocation.accuracyMeters)) {
            L.circle(userCoordinates, {
                radius: userLocation.accuracyMeters,
                color: "#23875a",
                weight: 1,
                fillColor: "#23875a",
                fillOpacity: 0.08
            }).addTo(map);
        }
    }

    if (bounds.length === 1)
        map.setView(bounds[0], 14);
    else if (bounds.length > 1)
        map.fitBounds(bounds, { padding: [36, 36], maxZoom: 14 });
    else
        map.setView([45.9432, 24.9668], 7);

    requestAnimationFrame(() => map.invalidateSize());
}

function createBusinessPopup(business) {
    const popup = document.createElement("div");
    popup.className = "business-map-popup";

    const type = document.createElement("span");
    type.className = "business-map-popup-type";
    type.textContent = business.businessTypeName || "Business";

    const name = document.createElement("strong");
    name.textContent = business.name;

    const address = document.createElement("span");
    address.textContent = business.address;

    popup.append(type, name, address);

    if (business.distanceLabel) {
        const distance = document.createElement("span");
        distance.className = "business-map-popup-distance";
        distance.textContent = business.distanceLabel;
        popup.append(distance);
    }

    const link = document.createElement("a");
    link.href = `/business/${business.id}`;
    link.textContent = "View business";
    popup.append(link);

    return popup;
}

export function disposeBusinessMap(elementId) {
    const map = maps.get(elementId);
    if (!map)
        return;

    map.remove();
    maps.delete(elementId);
}
