# MythAPI - Gods API Documentation

## Base URL

```
/api/v1/gods
```

## Endpoints

### Get All Gods

Retrieves a list of all gods.

- **URL:** `/api/v1/gods`
- **Method:** `GET`
- **Parameters:** None

#### Response

- **Status:** `200 OK`
- **Body:** Array of `God` objects

```json
[
  {
    "id": 1,
    "name": "Zeus",
    "description": "Zeus is the sky and thunder god in ancient Greek religion.",
    "mythologyId": 1,
    "aliases": []
  }
]
```

---

### Get God by ID

Retrieves a single god by its unique identifier.

- **URL:** `/api/v1/gods/{id}`
- **Method:** `GET`
- **Path Parameters:**

| Parameter | Type  | Required | Description              |
|-----------|-------|----------|--------------------------|
| `id`      | `int` | Yes      | The unique ID of the god |

#### Response

- **Status:** `200 OK`
- **Body:** A single `God` object

```json
{
  "id": 1,
  "name": "Zeus",
  "description": "Zeus is the sky and thunder god in ancient Greek religion.",
  "mythologyId": 1,
  "aliases": []
}
```

---

### Search Gods by Name

Searches for gods whose name matches the provided search term. Optionally includes alias matches.

- **URL:** `/api/v1/gods/search/{name}`
- **Method:** `GET`
- **Path Parameters:**

| Parameter | Type     | Required | Description                  |
|-----------|----------|----------|------------------------------|
| `name`    | `string` | Yes      | The name or partial name to search for |

- **Query Parameters:**

| Parameter        | Type   | Required | Default | Description                                  |
|------------------|--------|----------|---------|----------------------------------------------|
| `includeAliases` | `bool` | No       | `false` | When `true`, also searches within god aliases |

#### Response

- **Status:** `200 OK`
- **Body:** Array of matching `God` objects

```json
[
  {
    "id": 1,
    "name": "Zeus",
    "description": "Zeus is the sky and thunder god in ancient Greek religion.",
    "mythologyId": 1,
    "aliases": []
  }
]
```

---

### Add or Update Gods

Creates new gods or updates existing ones. If a god object includes an `id` that already exists, it will be updated; otherwise a new god is created.

- **URL:** `/api/v1/gods`
- **Method:** `POST`
- **Request Body:** Array of `GodInput` objects

```json
[
  {
    "id": null,
    "name": "Athena",
    "description": "Goddess of wisdom and warfare.",
    "mythologyId": 1
  },
  {
    "id": 2,
    "name": "Hades",
    "description": "Updated description for Hades.",
    "mythologyId": 1
  }
]
```

| Field         | Type   | Required | Description                                              |
|---------------|--------|----------|----------------------------------------------------------|
| `id`          | `int?` | No       | If provided and exists, the god is updated; otherwise created |
| `name`        | `string` | Yes    | The name of the god                                      |
| `description` | `string` | Yes    | A description of the god                                 |
| `mythologyId` | `int`  | Yes      | The ID of the mythology the god belongs to               |

#### Response

- **Status:** `200 OK`
- **Body:** Array of all `God` objects after the operation

---

## Models

### God

| Field         | Type       | Description                            |
|---------------|------------|----------------------------------------|
| `id`          | `int`      | Unique identifier                      |
| `name`        | `string`   | Name of the god                        |
| `description` | `string`   | Description of the god                 |
| `mythologyId` | `int`      | ID of the associated mythology         |
| `aliases`     | `Alias[]`  | List of alternative names for the god  |

### GodInput

| Field         | Type    | Description                                              |
|---------------|---------|----------------------------------------------------------|
| `id`          | `int?`  | Optional; if set and exists, the god is updated          |
| `name`        | `string`| Name of the god                                         |
| `description` | `string`| Description of the god                                  |
| `mythologyId` | `int`   | ID of the associated mythology                           |
