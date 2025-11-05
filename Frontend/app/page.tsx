import { redirect } from 'next/navigation'

export default function Index() {
  // Make login the default root page
  redirect('/login')
}
