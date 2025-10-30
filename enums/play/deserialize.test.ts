import { test, expect } from 'vitest'

enum OrderStatusStringEnum {
    Pending = 'Pending',
    Preparing = 'Preparing',
    InDelivery = 'InDelivery',
    Delivered = 'Delivered',
    Cancelled = 'Cancelled'
}

enum OrderStatusIntEnum {
    Pending = 1,
    Preparing = 2,
    InDelivery = 3,
    Delivered = 4,
    Cancelled = 5
}

enum OrderStatusAutoEnum {
    Pending,
    Preparing,
    InDelivery,
    Delivered,
    Cancelled,
}

type Order = {
    id?: number
    status: OrderStatusStringEnum
}

function deserializePrinted(json: string): Order {
    const order = JSON.parse(json) as Order

    console.log(order);
    return order;
}

test('simple string', () => {
    const json = '{ "id" : 5, "status" : "InDelivery" }';
    const order = deserializePrinted(json)
    expect(order.status).toBe(OrderStatusStringEnum.InDelivery);
})

test('capitalized', () => {
    const json = '{ "id" : 5, "status" : "INDELIVERY" }';
    const order = deserializePrinted(json)
    expect(order.status).not.toBe(OrderStatusStringEnum.InDelivery);
})

test('camelCased', () => {
    const json = '{ "id" : 5, "status" : "inDelivery" }';
    const order = deserializePrinted(json)
    expect(order.status).not.toBe(OrderStatusStringEnum.InDelivery);
})

test('numbers', () => {
    const json = '{ "id" : 5, "status" : 3 }';
    const order = deserializePrinted(json)
    expect(order.status).toBe(OrderStatusIntEnum.InDelivery);
})

test('numbers from string', () => {
    const json = '{ "id" : 5, "status" : "InDelivery" }';
    const order = deserializePrinted(json)
    expect(order.status).not.toBe(OrderStatusIntEnum.InDelivery);
})