import { WorkOrderDto, WorkOrderItemDto } from '../types'

/**
 * Calculates the total amount for a work order from its items.
 * Formula: sum(quantity * unitPrice * (1 + taxRate)) for each item
 * Falls back to order.totalAmount if items are not available or empty.
 */
export function getWorkOrderTotal(
  order: WorkOrderDto,
  items?: WorkOrderItemDto[]
): number {
  // If items are provided and not empty, calculate from items
  if (items && items.length > 0) {
    const calculatedTotal = items.reduce((sum, item) => {
      const itemTotal = item.quantity * item.unitPrice * (1 + item.taxRate)
      return sum + itemTotal
    }, 0)
    return calculatedTotal
  }

  // Fallback to order.totalAmount
  return order.totalAmount || 0
}

/**
 * Formats a number as currency with 2 decimal places
 */
export function formatCurrency(amount: number): string {
  return `$${amount.toFixed(2)}`
}
