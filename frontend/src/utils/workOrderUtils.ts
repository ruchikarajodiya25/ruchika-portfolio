/**
 * Converts tax rate from percent to decimal fraction for calculations.
 * Tax rates are stored as percent (0-100) in backend.
 * Examples:
 * - 8 => 0.08
 * - 5 => 0.05
 * - 0 => 0
 */
export function getDecimalFraction(taxRatePercent: number): number {
  return taxRatePercent / 100
}

/**
 * Calculates the total amount for an item including tax.
 * Formula: (quantity * unitPrice) * (1 + taxRatePercent/100)
 * Tax rate is expected as percent (0-100), e.g., 8 for 8%
 */
export function calculateItemTotal(quantity: number, unitPrice: number, taxRatePercent: number): number {
  const taxDecimal = getDecimalFraction(taxRatePercent)
  return quantity * unitPrice * (1 + taxDecimal)
}

/**
 * Calculates tax amount for an item.
 * Formula: (quantity * unitPrice) * (taxRatePercent/100)
 * Tax rate is expected as percent (0-100), e.g., 8 for 8%
 */
export function calculateTaxAmount(quantity: number, unitPrice: number, taxRatePercent: number): number {
  const taxDecimal = getDecimalFraction(taxRatePercent)
  return quantity * unitPrice * taxDecimal
}

import { WorkOrderDto, WorkOrderItemDto } from '../types'

/**
 * Calculates the total amount for a work order from its items.
 * PRIORITY: Always use backend-calculated totalAmount if > 0.
 * Only calculate from items if backend total is 0 AND items exist (temporary client-side items).
 * 
 * This function handles cases where:
 * - Backend totalAmount is set and > 0 (ALWAYS use it - backend computed from persisted items)
 * - Backend totalAmount is 0 but client-side items exist (temporary, calculate from them)
 * - Neither exists (return 0)
 */
export function getWorkOrderTotal(
  order: WorkOrderDto,
  items?: WorkOrderItemDto[]
): number {
  // ALWAYS prefer backend-calculated totalAmount if it exists and > 0
  // Backend computes this from persisted WorkOrderItems
  if (order.totalAmount && order.totalAmount > 0) {
    return order.totalAmount
  }

  // Only calculate from items if backend total is 0 AND we have items
  // This handles temporary client-side items that haven't been saved yet
  const itemsToUse = items || order.items || []
  
  if (itemsToUse.length > 0) {
    const calculatedTotal = itemsToUse.reduce((sum, item) => {
      return sum + calculateItemTotal(item.quantity, item.unitPrice, item.taxRate)
    }, 0)
    // Round to 2 decimal places
    return Math.round(calculatedTotal * 100) / 100
  }

  // No items and no backend total - return 0
  return 0
}

/**
 * Formats a number as currency with 2 decimal places
 */
export function formatCurrency(amount: number): string {
  return `$${amount.toFixed(2)}`
}

/**
 * Formats a tax rate for display (tax rate is stored as percent, e.g., 8 for 8%)
 * Example: 8 => "8.00%"
 */
export function formatTaxRate(taxRatePercent: number): string {
  return `${taxRatePercent.toFixed(2)}%`
}
