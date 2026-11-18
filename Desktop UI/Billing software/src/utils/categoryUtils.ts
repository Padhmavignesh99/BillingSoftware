import type { Category, Product } from '../types';

/** Returns only root (top-level) categories */
export function getRootCategories(categories: Category[]): Category[] {
  return categories.filter((c) => !c.parentCategoryId);
}

/** Returns direct children of a given parent id */
export function getSubCategories(categories: Category[], parentId: string): Category[] {
  return categories.filter((c) => c.parentCategoryId === parentId);
}

/** Returns all descendant ids of a category (including itself) */
export function getAllDescendantIds(categories: Category[], rootId: string): string[] {
  const result: string[] = [rootId];
  const children = getSubCategories(categories, rootId);
  for (const child of children) {
    result.push(...getAllDescendantIds(categories, child.id));
  }
  return result;
}

/** Returns the breadcrumb path of a category leaf, e.g. "Mens > Shirt > Full Hand" */
export function getLeafPath(categories: Category[], leafId: string): string {
  const parts: string[] = [];
  let current = categories.find((c) => c.id === leafId);
  while (current) {
    parts.unshift(current.name);
    current = current.parentCategoryId
      ? categories.find((c) => c.id === current!.parentCategoryId)
      : undefined;
  }
  return parts.join(' > ');
}

/**
 * Returns true if the product belongs to the filtered category OR
 * any of its descendants (so selecting "Mens" shows all Mens products).
 */
export function matchesCategory(
  product: Product,
  filterCategoryId: string,
  categories: Category[]
): boolean {
  const descendantIds = getAllDescendantIds(categories, filterCategoryId);
  // Match by id fields first, then fall back to name match
  if (product.categoryId && descendantIds.includes(product.categoryId)) return true;
  if (product.subCategoryId && descendantIds.includes(product.subCategoryId)) return true;
  if (product.subSubCategoryId && descendantIds.includes(product.subSubCategoryId)) return true;
  // Fallback: name-based match for legacy products
  const filterCat = categories.find((c) => c.id === filterCategoryId);
  if (!filterCat) return false;
  const catName = filterCat.name.toLowerCase();
  return product.category.toLowerCase() === catName ||
    (product.subCategory ?? '').toLowerCase() === catName ||
    (product.subSubCategory ?? '').toLowerCase() === catName;
}
