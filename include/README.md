## Examples

```http
GET transactions?include=total,amount_gte_0.total,category.total,category.totalSum
```

```json
{
    "count" : 5,
    "items" : [
        {
            "amount" : 100,
            "category" : "salary"
        },
        {
            "amount" : 200,
            "category" : "salary"
        },
        {
            "amount" : 300,
            "category" : "stocks"
        },
        {
            "amount" : -100,
            "category" : "food"
        },
        {
            "amount" : -100,
            "category" : "stocks"
        },
    ],
    "total" : 5,
    "amount_gte_0" : {
        "yes" : {
            "total" : 3
        },
        "no" : {
            "total" : 2
        }
    },
    "category" : {
        "salary" : {
            "total" : 2,
            "totalSum" : 300
        },
        "stocks" : {
            "total" : 2,
            "totalSum" : 200
        },
        "food" : {
            "total" : 2,
            "totalSum" : -100
        }
    }
}
```


```http
GET transactions?limit=0&include=total,amount_gte_0.total,category.total,category.totalSum
```

```json
{
    "count" : 0,
    "items" : [],

    "total" : 5,
    "amount_gte_0" : {
        "yes" : {
            "total" : 3
        },
        "no" : {
            "total" : 2
        }
    },
    "category" : {
        "salary" : {
            "total" : 2,
            "totalSum" : 300
        },
        "stocks" : {
            "total" : 2,
            "totalSum" : 200
        },
        "food" : {
            "total" : 2,
            "totalSum" : -100
        }
    },
}
```
`include` logic:

- `total` means limits are not applied
- field paths with `_` are considered **expressions**
- Root has a number of known includes: `total`, `items` for now.
- Unknown root includes are considered group keys
    - By default nothing from the group is included, therefore it required at least one subsequent field (otherwise - 400)
    - If a group key is an expression it will have two group values `yes` and `no`

### Total field

Total should mean that **limit** filter is not applied